using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Cache;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.Helpers.AuthHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.DbServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.Infrastructure.Models.Kafka;
using BookingManagementService.Infrastructure.Services.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Claims;

namespace BookingManagementService.API.Controllers.BaseControllers
{
    [Route("api/bookings")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class BookingController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly BookingService _bookingService;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;

        public BookingController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient, 
            BookingService bookingService,
            JwtHelper jwtHelper,
            FileIOHelper fileIOHelper,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _bookingService = bookingService;
            _jwtHelper = jwtHelper;
            _fileIOHelper = fileIOHelper;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
        }

        [HttpGet]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetAllBookings()
        {
            var result = await _bookingService.GetAllBookingsAsync();

            if (result == null || !result.Any())
            {
                return NotFound("No bookings found.");
            }
            return Ok(result);
        }

        [HttpGet("{BookingId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetBookingById(int BookingId)
        {
            var result = await _bookingService.GetBookingByIdAsync(BookingId);
            if (result == null)
            {
                return NotFound($"Booking with ID {BookingId} not found.");
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "Title", request.BookingInfo.Title },
                { "Description", request.BookingInfo.Description },
                { "AccountId", accountId },
                { "PodcastBuddyId", request.BookingInfo.PodcastBuddyId }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if(!result)
            {
                return StatusCode(500, "Failed to initiate booking creation process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId,
            });
        }

        // NEW: Booking Negotiation Multipart Endpoint
        [HttpPost("{BookingId}/book-negotiations")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> CreateBookingNegotiation(
            [FromRoute] int BookingId,
            [FromForm] BookingNegotiationRequestDTO bookingNegotiationRequestDTO)
        {
            try
            {
                var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
                var accountId = account.Id;

                var isValid = await _bookingService.ValidateBookingAccountOrPodcasterAsync(BookingId, accountId);
                if(!isValid)
                {
                    throw new UnauthorizedAccessException("You are not authorized to create booking negotiate for this booking.");
                }
                // Parse the JSON BookingNegotiationInfo
                var negotiationRequestInfo = JsonConvert.DeserializeObject<BookingNegotiationInfoDTO>(bookingNegotiationRequestDTO.BookingNegotiationInfo);
                if (negotiationRequestInfo == null)
                {
                    return BadRequest("Invalid BookingNegotiationInfo format.");
                }

                string demoAudioFileKey = null;
                if (bookingNegotiationRequestDTO.DemoAudioFile != null)
                {
                    var isValidAudioFile = _fileValidationConfig.IsValidFile(
                        "BookingNegotiation.demoAudioFileKey", 
                        bookingNegotiationRequestDTO.DemoAudioFile.FileName, 
                        bookingNegotiationRequestDTO.DemoAudioFile.Length, 
                        bookingNegotiationRequestDTO.DemoAudioFile.ContentType);
                    if (!isValidAudioFile)
                    {
                        return BadRequest("Invalid audio file. Please ensure the file type and size are correct.");
                    }
                    string newDemoAudioFileName = $"{Guid.NewGuid()}_{bookingNegotiationRequestDTO.DemoAudioFile.FileName}";
                    using (var memoryStream = bookingNegotiationRequestDTO.DemoAudioFile.OpenReadStream())
                    {                        
                        await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                            memoryStream,
                            _filePathConfig.BOOKING_TEMP_FILE_PATH,
                            newDemoAudioFileName);                    }
                    demoAudioFileKey = FilePathHelper.CombinePaths(_filePathConfig.BOOKING_TEMP_FILE_PATH, newDemoAudioFileName);
                }
                JObject requestData = JObject.FromObject(negotiationRequestInfo);
                requestData["AccountId"] = accountId;
                requestData["BookingId"] = BookingId;
                requestData["DemoAudioFileKey"] = demoAudioFileKey;

                var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-negotiation-flow");
                var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
                if (!result)
                {
                    return StatusCode(500, "Failed to initiate booking negotiation process.");
                }
                return Ok(new
                {
                    SagaInstanceId = startSagaTriggerMessage.SagaInstanceId,
                }
                );
            }
            catch (JsonException)
            {
                return BadRequest("Invalid JSON format in BookingNegotiationInfo parameter.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the booking negotiation: {ex.Message}");
            }
        }

        [HttpGet("me")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetMyBookings()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var result = await _bookingService.GetBookingsByAccountIdAsync(accountId);
            if (result == null || !result.Any())
            {
                return NotFound("No bookings found for the current user.");
            }
            return Ok(result);
        }

        [HttpPut("{BookingId}/reject")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> RejectBooking([FromRoute] int BookingId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _bookingService.ValidateBookingPodcasterAsync(BookingId, accountId);
            if (!isValid)
            {
                throw new UnauthorizedAccessException("You are not authorized to reject this booking.");
            }

            var requestData = new JObject
            {
                { "BookingId", BookingId }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-reject-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking creation process.");
            }
            return Ok(new {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        [HttpPut("{BookingId}/cancel")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> CancelBooking([FromRoute] int BookingId, [FromBody] BookingCancelRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _bookingService.ValidateBookingAccountAsync(BookingId, accountId);
            if (!isValid)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");
            }

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId },
                { "BookingManualCancelledReason", request.ManualBookingCancelledReason }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-manual-cancellation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking creation process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
