using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.DTOs.Cache;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.DbServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.Infrastructure.Models.Audio.AcoustID;
using BookingManagementService.Infrastructure.Services.Audio.AcoustID;
using BookingManagementService.Infrastructure.Services.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BookingManagementService.API.Controllers.BaseControllers
{
    [Route("api/producing-requests")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class ProducingRequestController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly BookingProducingRequestService _bookingProducingRequestService;
        private readonly BookingService _bookingService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly AcoustIDAudioFingerprintGenerator _audioFingerprintGenerator;
        private readonly ILogger<ProducingRequestController> _logger;

        public ProducingRequestController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            BookingProducingRequestService bookingProducingRequestService,
            BookingService bookingService,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            AcoustIDAudioFingerprintGenerator audioFingerprintGenerator,
            ILogger<ProducingRequestController> logger)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _bookingProducingRequestService = bookingProducingRequestService;
            _bookingService = bookingService;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _audioFingerprintGenerator = audioFingerprintGenerator;
            _logger = logger;
        }

        [HttpGet("{BookingProducingRequestId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetProducingRequestById([FromRoute] Guid BookingProducingRequestId)
        {
            var result = await _bookingProducingRequestService.GetProducingRequestByIdAsync(BookingProducingRequestId);
            if (result == null)
            {
                return NotFound($"Booking Producing Request with ID {BookingProducingRequestId} not found.");
            }
            return Ok(result);
        }

        [HttpPost("bookings/{BookingId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> CreateProducingRequest([FromRoute] int BookingId, [FromBody] BookingProducingRequestRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account == null)
            {
                return Unauthorized("Account information not found.");
            }

            var accountId = account.Id;
            var isValid = await _bookingService.ValidateBookingAccountAsync(BookingId, accountId);
            if (!isValid)
            {
                return Forbid("You are not authorized to create booking producing request for this booking.");
            }

            var requestData = new JObject
            {
                { "BookingId", BookingId },
                { "Note", request.BookingProducingRequestInfo.Note },
                { "Deadline", request.BookingProducingRequestInfo.Deadline.ToString() }, // Convert DateOnly to string
                { "BookingPodcastTrackIds", JArray.FromObject(request.BookingProducingRequestInfo.BookingPodcastTrackIds) }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-producing-request-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking producing request creation process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        [HttpPut("{BookingProducingRequestId}/submit")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> SubmitAudioTrack(
            [FromRoute] Guid BookingProducingRequestId,
            [FromForm] BookingPodcastTrackRequestDTO request
            )
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account == null)
            {
                return Unauthorized("Account information not found.");
            }

            var accountId = account.Id;
            var isValid = await _bookingProducingRequestService.ValidateProducingRequestPodcasterAsync(BookingProducingRequestId, accountId);
            if (!isValid)
            {
                return Forbid("You are not authorized to submit tracks for this booking producing request.");
            }

            // Validate all audio files first
            foreach (var audioFile in request.AudioFiles)
            {
                var isValidAudioFile = _fileValidationConfig.IsValidFile("BookingPodcastTrack.audioFileKey", audioFile.FileName, audioFile.Length, audioFile.ContentType);
                if (!isValidAudioFile)
                {
                    return BadRequest($"Invalid audio file '{audioFile.FileName}'. Please ensure all audio files have correct type and size.");
                }
            }

            var trackSubmissions = new List<JObject>();

            // Process all audio files and prepare track submission items
            foreach (var audioFile in request.AudioFiles)
            {
                string newTrackAudioFileName = $"{Guid.NewGuid()}_{audioFile.FileName}";
                Console.WriteLine($"Generated new audio file name: {newTrackAudioFileName}");
                AcoustIDAudioFingerprintGeneratedResult? audioMetadata = null;

                using (var memoryStream = audioFile.OpenReadStream())
                {
                    try
                    {
                        audioMetadata = await _audioFingerprintGenerator.GenerateFingerprintAsync(memoryStream);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with default metadata
                        _logger.LogError(ex, "Failed to generate audio fingerprint for file: {FileName}", audioFile.FileName);
                        audioMetadata = new AcoustIDAudioFingerprintGeneratedResult
                        {
                            Duration = 0,
                            FingerprintData = string.Empty
                        };
                    }

                    // Upload the file first
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(memoryStream, _filePathConfig.BOOKING_TEMP_FILE_PATH, newTrackAudioFileName);
                }

                var trackAudioFileKey = FilePathHelper.CombinePaths(_filePathConfig.BOOKING_TEMP_FILE_PATH, newTrackAudioFileName);

                // Add to track submissions list
                trackSubmissions.Add(new JObject
                {
                    { "AudioFileKey", trackAudioFileKey },
                    { "AudioFileSize", audioFile.Length },
                    { "AudioLength", (int)(audioMetadata?.Duration ?? 0) }
                });
            }

            // Create a single saga message with all tracks
            var requestData = new JObject
            {
                { "BookingProducingRequestId", BookingProducingRequestId },
                { "Tracks", JArray.FromObject(trackSubmissions) }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-track-submission-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);

            if (!result)
            {
                // If saga message fails, clean up uploaded files
                foreach (var track in trackSubmissions)
                {
                    try
                    {
                        var audioFileKey = track["AudioFileKey"]?.ToString();
                        if (!string.IsNullOrEmpty(audioFileKey))
                        {
                            await _fileIOHelper.DeleteFileAsync(audioFileKey);
                        }
                    }
                    catch (Exception ex)
                    {
                        var audioFileKey = track["AudioFileKey"]?.ToString();
                        _logger.LogError(ex, "Failed to delete temporary file: {AudioFileKey}", audioFileKey);
                    }
                }
                return StatusCode(500, "Failed to initiate booking producing request submission process.");
            }

            return Ok(new
            {
                //Message = "Booking producing request submitted successfully.",
                //TracksSubmitted = trackSubmissions.Count,
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        [HttpPut("{BookingProducingRequestId}/accept/{isAccepted}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> BookingProducingRequestAcceptance(
            [FromRoute] Guid BookingProducingRequestId,
            [FromRoute] bool isAccepted)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account == null)
            {
                return Unauthorized("Account information not found.");
            }
            var accountId = account.Id;
            var isValid = await _bookingProducingRequestService.ValidateProducingRequestPodcasterAsync(BookingProducingRequestId, accountId);
            if (!isValid)
            {
                return Forbid("You are not authorized to accept or reject this booking producing request.");
            }
            var requestData = new JObject
            {
                { "BookingProducingRequestId", BookingProducingRequestId },
                { "IsAccepted", isAccepted }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-producing-request-agreement-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking producing request acceptance process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId,
            });
        }

        [HttpPut("{BookingPodcastTrackId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> UpdateBookingPodcastTrackPreviewListenSlot(
            [FromRoute] Guid BookingPodcastTrackId)
        {
            var requestData = new JObject
            {
                { "BookingPodcastTrackId", BookingPodcastTrackId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("booking-management-domain", requestData, null, "booking-track-preview-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking podcast track preview update process.");
            }
            return Ok("Booking podcast track details updated successfully.");
        }
    }
}
