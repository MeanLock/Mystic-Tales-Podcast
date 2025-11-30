using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.DTOs.Auth;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Booking;
using BookingManagementService.BusinessLogic.DTOs.Cache;
using BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest;
using BookingManagementService.BusinessLogic.Enums.App;
using BookingManagementService.BusinessLogic.Enums.Kafka;
using BookingManagementService.BusinessLogic.Enums.ListenSessionProcedure;
using BookingManagementService.BusinessLogic.Helpers.AuthHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using BookingManagementService.BusinessLogic.Services.DbServices.BookingServices;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.Infrastructure.Models.Audio.AcoustID;
using BookingManagementService.Infrastructure.Models.Kafka;
using BookingManagementService.Infrastructure.Services.Audio.Hls;
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
    [Authorize(Policy = "OptionalAccess")]
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
        private const string SAGA_TOPIC = KafkaTopicEnum.BookingManagementDomain;
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;

        public BookingController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            BookingService bookingService,
            JwtHelper jwtHelper,
            FileIOHelper fileIOHelper,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            FFMpegCoreHlsService ffMpegCoreHlsService)
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
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetAllBookings()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var roleId = account.RoleId;
            var result = await _bookingService.GetAllBookingsAsync(accountId, roleId);

            //if (result == null || !result.Any())
            //{
            //    return NotFound("No bookings found.");
            //}
            return Ok(new
            {
                BookingList = result
            });
        }

        [HttpGet("{BookingId}")]
        [Authorize(Policy = "BasicAccess")]
        public async Task<IActionResult> GetBookingById(int BookingId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var roleId = account.RoleId;
            var result = await _bookingService.GetBookingByIdAsync(BookingId, accountId, roleId);
            //if (result == null)
            //{
            //    return NotFound($"Booking with ID {BookingId} not found.");
            //}
            return Ok(new
            {
                Booking = result
            });
        }

        [HttpPost]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> CreateBooking([FromForm] BookingCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var bookingCreateInfo = JsonConvert.DeserializeObject<BookingCreateInfoDTO>(request.BookingCreateInfo);

            Console.WriteLine(JsonConvert.SerializeObject(request));

            // Validate all audio files first
            foreach (var requirementFile in request.BookingRequirementFiles)
            {
                var isValidRequirementFile = _fileValidationConfig.IsValidFile("BookingRequirement.requirementDocumentFileKey", requirementFile.FileName, requirementFile.Length, requirementFile.ContentType);
                if (!isValidRequirementFile)
                {
                    return BadRequest($"Invalid requirement document file '{requirementFile.FileName}'. Please ensure all requirement document files have correct type and size.");
                }
            }

            var requirementSubmission = JArray.FromObject(bookingCreateInfo.BookingRequirementInfo);
            var requirementDocumentSubmission = new List<JObject>();

            // First, process uploaded files and map them to requirements
            var fileKeysByOrder = new Dictionary<int, string>();
            foreach (var requirementFile in request.BookingRequirementFiles)
            {
                string newRequirementFileName = $"{Guid.NewGuid()}_{requirementFile.FileName}";
                Console.WriteLine($"Generated new requirement file name: {newRequirementFileName}");

                using (var memoryStream = requirementFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(memoryStream, _filePathConfig.BOOKING_TEMP_FILE_PATH, newRequirementFileName);
                }

                var requirementDocumentFileKey = FilePathHelper.CombinePaths(_filePathConfig.BOOKING_TEMP_FILE_PATH, newRequirementFileName);

                // Extract order from filename (assuming the filename starts with the order number)
                var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(requirementFile.FileName);
                if (int.TryParse(fileNameWithoutExtension, out int order))
                {
                    fileKeysByOrder[order] = requirementDocumentFileKey;
                }
            }

            // Process all requirements (with or without files)
            foreach (var requirement in requirementSubmission)
            {
                var requirementCopy = (JObject)requirement.DeepClone();

                if (requirement["Order"] != null && int.TryParse(requirement["Order"].ToString(), out int order))
                {
                    // Check if this requirement has an associated file
                    if (fileKeysByOrder.TryGetValue(order, out string fileKey))
                    {
                        requirementCopy["RequirementDocumentFileKey"] = fileKey;
                    }
                    else
                    {
                        requirementCopy["RequirementDocumentFileKey"] = null;
                    }
                }
                else
                {
                    requirementCopy["RequirementDocumentFileKey"] = null;
                }

                requirementDocumentSubmission.Add(requirementCopy);
            }

            var requestData = new JObject
            {
                { "Title", bookingCreateInfo.Title },
                { "Description", bookingCreateInfo.Description },
                { "AccountId", accountId },
                { "PodcastBuddyId", bookingCreateInfo.PodcastBuddyId },
                { "DeadlineDayCount", bookingCreateInfo.DeadlineDayCount },
                { "BookingRequirementInfoList", JArray.FromObject(requirementDocumentSubmission) },
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking creation process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId,
            });
        }

        [HttpGet("podcast-booking-tone")]
        public async Task<IActionResult> GetPodcastBookingTones()
        {
            var result = await _bookingService.GetAllPodcastBookingTonesAsync();
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No podcast booking tones found.");
            //}
            return Ok(new
            {
                PodcastBookingToneList = result
            });
        }
        [HttpGet("podcast-booking-tone/me")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetPodcasterPodcastBookingTones()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var result = await _bookingService.GetPodcasterPodcastBookingTonesAsync(accountId);
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No podcast booking tones found.");
            //}
            return Ok(new
            {
                PodcastBookingToneList = result
            });
        }
        [HttpGet("podcast-booking-tone/{PodcastBookingToneId}/podcast-buddy")]
        public async Task<IActionResult> GetPodcasterByBookingToneId([FromRoute] Guid PodcastBookingToneId)
        {
            var result = await _bookingService.GetPodcastersByBookingToneIdAsync(PodcastBookingToneId);
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No podcast booking tones found.");
            //}
            return Ok(new
            {
                PodcastBuddyList = result
            });
        }

        [HttpGet("{BookingId}/requirement")]
        public async Task<IActionResult> GetAllBookingRequirementByBookingId([FromRoute] int BookingId)
        {
            var result = await _bookingService.GetAllBookingRequirementByBookingIdAsync(BookingId);
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No podcast booking tones found.");
            //}
            return Ok(new
            {
                BookingRequirementList = result
            });
        }
        // NEW: Booking Negotiation Multipart Endpoint
        //[HttpPost("{BookingId}/book-negotiations")]
        //[Authorize(Policy = "Customer.BasicAccess")]
        //public async Task<IActionResult> CreateBookingNegotiation(
        //    [FromRoute] int BookingId,
        //    [FromForm] BookingNegotiationCreateRequestDTO bookingNegotiationRequestDTO)
        //{
        //    try
        //    {
        //        var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //        var accountId = account.Id;

        //        var isValid = await _bookingService.ValidateBookingAccountOrPodcasterAsync(BookingId, accountId);
        //        if(!isValid)
        //        {
        //            throw new UnauthorizedAccessException("You are not authorized to create booking negotiate for this booking.");
        //        }
        //        // Parse the JSON BookingNegotiationInfo
        //        var negotiationRequestInfo = JsonConvert.DeserializeObject<BookingNegotiationInfoDTO>(bookingNegotiationRequestDTO.BookingNegotiationInfo);
        //        if (negotiationRequestInfo == null)
        //        {
        //            return BadRequest("Invalid BookingNegotiationInfo format.");
        //        }

        //        string demoAudioFileKey = null;
        //        if (bookingNegotiationRequestDTO.DemoAudioFile != null)
        //        {
        //            var isValidAudioFile = _fileValidationConfig.IsValidFile(
        //                "BookingNegotiation.demoAudioFileKey", 
        //                bookingNegotiationRequestDTO.DemoAudioFile.FileName, 
        //                bookingNegotiationRequestDTO.DemoAudioFile.Length, 
        //                bookingNegotiationRequestDTO.DemoAudioFile.ContentType);
        //            if (!isValidAudioFile)
        //            {
        //                return BadRequest("Invalid audio file. Please ensure the file type and size are correct.");
        //            }
        //            string newDemoAudioFileName = $"{Guid.NewGuid()}_{bookingNegotiationRequestDTO.DemoAudioFile.FileName}";
        //            using (var memoryStream = bookingNegotiationRequestDTO.DemoAudioFile.OpenReadStream())
        //            {                        
        //                await _fileIOHelper.UploadBinaryFileWithStreamAsync(
        //                    memoryStream,
        //                    _filePathConfig.BOOKING_TEMP_FILE_PATH,
        //                    newDemoAudioFileName);                    }
        //            demoAudioFileKey = FilePathHelper.CombinePaths(_filePathConfig.BOOKING_TEMP_FILE_PATH, newDemoAudioFileName);
        //        }
        //        JObject requestData = JObject.FromObject(negotiationRequestInfo);
        //        requestData["AccountId"] = accountId;
        //        requestData["BookingId"] = BookingId;
        //        requestData["DemoAudioFileKey"] = demoAudioFileKey;

        //        var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //            topic: SAGA_TOPIC, 
        //            requestData: requestData, 
        //            sagaInstanceId: null, 
        //            messageName: "booking-negotiation-flow");
        //        var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //        if (!result)
        //        {
        //            return StatusCode(500, "Failed to initiate booking negotiation process.");
        //        }
        //        return Ok(new
        //        {
        //            SagaInstanceId = startSagaTriggerMessage.SagaInstanceId,
        //        }
        //        );
        //    }
        //    catch (JsonException)
        //    {
        //        return BadRequest("Invalid JSON format in BookingNegotiationInfo parameter.");
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while creating the booking negotiation: {ex.Message}");
        //    }
        //}
        [HttpPost("{BookingId}/dealing")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> BookingDealing(
            [FromRoute] int BookingId, [FromBody] BookingDealingRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _bookingService.ValidateBookingPodcasterAsync(BookingId, accountId);
            //if (!isValid)
            //{
            //    throw new UnauthorizedAccessException("You are not authorized to create booking dealing for this booking.");
            //}
            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId },
                { "BookingRequirementInfoList", JArray.FromObject(request.BookingDealingInfo.BookingRequirementInfoList) },
                { "DeadlineDayCount", request.BookingDealingInfo.DeadlineDayCount },
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-dealing-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking dealing process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("podcaster")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetPodcasterBookings()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var result = await _bookingService.GetBookingsByPodcasterIdAsync(accountId);
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No bookings found for the current podcaster.");
            //}
            return Ok(new
            {
                BookingList = result
            });
        }

        [HttpGet("me")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetMyBookings()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isPodcaster = account.HasVerifiedPodcasterProfile && account.PodcasterProfileIsBuddy;

            var result = await _bookingService.GetBookingsByAccountIdAsync(accountId, isPodcaster);
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No bookings found for the current user.");
            //}
            return Ok(new
            {
                BookingList = result
            });
        }

        [HttpPut("{BookingId}/reject")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> RejectBooking([FromRoute] int BookingId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            //var isValid = await _bookingService.ValidateBookingPodcasterAsync(BookingId, accountId);
            //if (!isValid)
            //{
            //    throw new UnauthorizedAccessException("You are not authorized to reject this booking.");
            //}

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-reject-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking creation process.");
            }
            return Ok(new
            {
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

            //var isValid = await _bookingService.ValidateBookingAccountAsync(BookingId, accountId);
            //if (!isValid)
            //{
            //    throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");
            //}

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId },
                { "BookingManualCancelledReason", request.BookingCancelledReason }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-manual-cancellation-flow");
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

        // /api/booking-management-service/api/bookings/{BookingId}/booking-podcast-tracks/{BookingPodcastTrackId}/listen
        [HttpPost("{BookingId}/booking-podcast-tracks/{BookingPodcastTrackId}/listen")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> MarkBookingPodcastTrackAsListened(int BookingId, Guid BookingPodcastTrackId, [FromBody] BookingTrackListenRequestDTO request)
        {
            string deviceTokenHeader = Request.Headers["X-DeviceInfo-Token"];
            string authorizedDeviceToken = HttpContext.User.FindFirst("device_info_token")?.Value;
            if (string.IsNullOrEmpty(deviceTokenHeader))
            {
                return BadRequest(new
                {
                    // error = "Missing X-Device-Fingerprint header"
                    error = "Missing X-DeviceInfo-Token header"
                });
            }
            else if (string.IsNullOrEmpty(authorizedDeviceToken))
            {
                return Unauthorized(new
                {
                    // error = "Unauthorized: Missing device_fingerprint claim"
                    error = "Unauthorized: Missing device_info_token claim"
                });
            }
            else if (deviceTokenHeader != authorizedDeviceToken)
            {
                return Unauthorized(new
                {
                    // error = "Unauthorized: Device fingerprint mismatch"
                    error = "Unauthorized: Device info token mismatch"
                });
            }
            var deviceInfo = JwtHelper.ClaimsPrincipalToObject<DeviceInfoDTO>(_jwtHelper.DecodeToken_OneSecretKey(deviceTokenHeader));


            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var trackListenResponse = await _bookingService.GetTrackListenAsync(BookingId, BookingPodcastTrackId, account.Id, deviceInfo, request.SourceType);

            return Ok(trackListenResponse);
        }

        // /api/booking-management-service/api/bookings/{BookingId}/booking-podcast-tracks/{BookingPodcastTrareckId}/hls-encryption-key/{KeyId}
        [HttpGet("{BookingId}/booking-podcast-tracks/{BookingPodcastTrackId}/hls-encryption-key/{KeyId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetBookingPodcastTrackHlsEncryptionKeyFileUrl(int BookingId, Guid BookingPodcastTrackId, Guid KeyId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var encryptionKeyBytes = await _bookingService.GetBookingTrackHlsEncryptionKeyFileAsync(BookingId, BookingPodcastTrackId, KeyId, account.Id);

            Response.Headers.CacheControl = "no-store";
            return File(encryptionKeyBytes, "application/octet-stream", enableRangeProcessing: false);
        }

        // /api/booking-management-service/api/bookings/{BookingId}/booking-podcast-tracks/hls-playlist/get-file-data/{**FileKey}
        [HttpGet("{BookingId}/booking-podcast-tracks/hls-playlist/get-file-data/{**FileKey}")]
        public async Task<IActionResult> GetHlsPlaylistFileUrl(int BookingId, string FileKey)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // Validate file key phải là HLS playlist
            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.BookingPodcastTrackPlaylist)
            {
                return StatusCode(403, new
                {
                    error = "Invalid file key: Must be an HLS playlist file",
                    actualCategory = category.ToString()
                });
            }

            // Generate presigned URL (2 minutes expiration)
            var fileData = await _fileIOHelper.GetFileBytesAsync(FileKey);
            var segmentRootPath = FilePathHelper.GetFolderPathFromFilePath(FileKey);
            string fileString = _ffMpegCoreHlsService.GetPlaylistContentAsync(fileData, segmentRootPath);
            Response.Headers.CacheControl = "no-store";

            return Content(fileString, "application/vnd.apple.mpegurl");
        }

        // /api/booking-management-service/api/bookings/{BookingId}/booking-podcast-tracks/hls-segment/get-file-data/{**FileKey}
        [HttpGet("{BookingId}/booking-podcast-tracks/hls-segment/get-file-data/{**FileKey}")]
        public async Task<IActionResult> GetHlsSegmentFileUrl(int BookingId, string FileKey)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // Validate file key phải là HLS segment
            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.BookingPodcastTrackSegment)
            {
                return StatusCode(403, new
                {
                    error = "Invalid file key: Must be an HLS segment file",
                    actualCategory = category.ToString()
                });
            }

            // Generate presigned URL (2 minutes expiration)
            var fileData = await _fileIOHelper.GetFileBytesAsync(FileKey);
            if (fileData == null)
                return NotFound("Unable to read segment");

            return File(fileData, "video/MP2T");
        }
        [HttpPost("podcast-booking-tone/me/{isBuddy}")]
        public async Task<IActionResult> CreatePodcastBookingTone(
            [FromRoute] bool isBuddy,
            [FromBody] PodcasterBookingToneApplyRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            if (!account.HasVerifiedPodcasterProfile || !account.PodcasterProfileIsBuddy)
            {
                throw new HttpRequestException("Only verified podcasters can create podcast booking tones.");
            }

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "IsBuddy", isBuddy },
                { "PodcastToneIds", JArray.FromObject(request.PodcasterBookingToneApplyInfo) }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-podcast-tone-apply-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcaster booking tone apply process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("{BookingId}/producing-request")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> CreateProducingRequest([FromRoute] int BookingId, [FromBody] BookingProducingRequestCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (account == null)
            {
                return Unauthorized("Account information not found.");
            }

            var accountId = account.Id;
            //var isValid = await _bookingService.ValidateBookingAccountAsync(BookingId, accountId);
            //if (!isValid)
            //{
            //    return Forbid("You are not authorized to create booking producing request for this booking.");
            //}

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId },
                { "Note", request.BookingProducingRequestInfo.Note },
                { "DeadlineDayCount", request.BookingProducingRequestInfo.DeadlineDayCount }, // Convert DateOnly to string
                { "BookingPodcastTrackIds", JArray.FromObject(request.BookingProducingRequestInfo.BookingPodcastTrackIds) }
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-producing-request-creation-flow");
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
        [HttpPost("{BookingId}/cancel-request")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CancelProducingRequest(
            [FromRoute] int BookingId,
            [FromBody] BookingCancelRequestRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _bookingService.ValidateBookingAccountAsync(BookingId, accountId);
            //if (!isValid)
            //{
            //    return Forbid("You are not authorized to cancel producing request for this booking.");
            //}
            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId },
                { "BookingManualCancelledReason", request.BookingCancelInfo.BookingManualCancelledReason }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-producing-request-cancellation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking producing request cancellation process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPut("{BookingId}/cancel-request/{IsAccepted}")]
        [Authorize(Policy = "Staff.BasicAccess")]
        public async Task<IActionResult> ProcessCancelRequest(
            [FromRoute] int BookingId,
            [FromRoute] bool IsAccepted,
            [FromBody] BookingCancelValidationRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId },
                { "IsAccepted", IsAccepted },
                { "CustomerBookingCancelDepositRefundRate", request.BookingCancelValidationInfo.CustomerBookingCancelDepositRefundRate },
                { "PodcastBuddyBookingCancelDepositRefundRate", request.BookingCancelValidationInfo.PodcastBuddyBookingCancelDepositRefundRate }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-cancel-validation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking producing request cancel validation process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("completed")]
        [Authorize("BasicAccess")]
        public async Task<IActionResult> GetCompletedBookings()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isPodcaster = account.HasVerifiedPodcasterProfile && account.PodcasterProfileIsBuddy;
            var result = await _bookingService.GetCompletedBookingsByAccountIdAsync(accountId, isPodcaster);
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No completed bookings found for the current user.");
            //}
            return Ok(new
            {
                BookingList = result
            });
        }
        [HttpGet("get-completed-booking/{BookingId}")]
        [Authorize("BasicAccess")]
        public async Task<IActionResult> GetCompletedBookingDetail([FromRoute] int BookingId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var result = await _bookingService.GetCompletedBookingDetailByAccountIdAsync(accountId, BookingId);
            //if (result == null || !result.Any())
            //{
            //    return NotFound("No completed bookings found for the current user.");
            //}
            return Ok(new
            {
                BookingList = result
            });
        }
        [HttpGet("{BookingId}/result")]
        [Authorize("BasicAccess")]
        public async Task<IActionResult> GetBookingResultById(int BookingId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var result = await _bookingService.GetBookingResultByIdAsync(BookingId, accountId);
            //if (result == null)
            //{
            //    return NotFound($"Booking result with ID {BookingId} not found.");
            //}
            return Ok(new
            {
                BookingResult = result
            });
        }
        [HttpGet("listen-sessions/latest")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetLatestBookingPodcastTrackListenSession()
        {
            string deviceTokenHeader = Request.Headers["X-DeviceInfo-Token"];
            string authorizedDeviceToken = HttpContext.User.FindFirst("device_info_token")?.Value;
            if (string.IsNullOrEmpty(deviceTokenHeader))
            {
                return BadRequest(new
                {
                    error = "Missing X-Device-Fingerprint header"
                });
            }
            else if (string.IsNullOrEmpty(authorizedDeviceToken))
            {
                return Unauthorized(new
                {
                    error = "Unauthorized: Missing device_fingerprint claim"
                });
            }
            else if (deviceTokenHeader != authorizedDeviceToken)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized: Device fingerprint mismatch"
                });
            }
            var deviceInfo = JwtHelper.ClaimsPrincipalToObject<DeviceInfoDTO>(_jwtHelper.DecodeToken_OneSecretKey(deviceTokenHeader));

            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var listenSession = await _bookingService.GetLatestBookingPodcastTrackListenSessionAsync(account.Id, deviceInfo);

            return Ok(listenSession);
        }
        [HttpPut("listen-sessions/{BookingPodcastTrackListenSessionId}/last-duration-seconds/{LastListenDurationSeconds}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> UpdatePodcastEpisodeListenSessionLastDurationSeconds(Guid BookingPodcastTrackListenSessionId, int LastListenDurationSeconds)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["BookingPodcastTrackListenSessionId"] = BookingPodcastTrackListenSessionId,
                ["ListenerId"] = account.Id,
                ["LastListenDurationSeconds"] = LastListenDurationSeconds,
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(KafkaTopicEnum.BookingManagementDomain, requestData, null, "booking-listen-session-duration-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("listen-sessions/navigate")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> NavigateBookingPodcastTrackListenSession([FromBody] BookingPodcastTrackListenSessionNavigateRequestDTO request, [FromQuery] ListenSessionNavigateTypeEnum listen_session_navigate_type)
        {
            string deviceTokenHeader = Request.Headers["X-DeviceInfo-Token"];
            string authorizedDeviceToken = HttpContext.User.FindFirst("device_info_token")?.Value;
            if (string.IsNullOrEmpty(deviceTokenHeader))
            {
                return BadRequest(new
                {
                    error = "Missing X-Device-Fingerprint header"
                });
            }
            else if (string.IsNullOrEmpty(authorizedDeviceToken))
            {
                return Unauthorized(new
                {
                    error = "Unauthorized: Missing device_fingerprint claim"
                });
            }
            else if (deviceTokenHeader != authorizedDeviceToken)
            {
                return Unauthorized(new
                {
                    error = "Unauthorized: Device fingerprint mismatch"
                });
            }
            var deviceInfo = JwtHelper.ClaimsPrincipalToObject<DeviceInfoDTO>(_jwtHelper.DecodeToken_OneSecretKey(deviceTokenHeader));
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var listenSession = await _bookingService.NavigateBookingPodcastTrackListenSessionAsync(account.Id, deviceInfo, request.CurrentListenSession.ListenSessionId, request.CurrentListenSession.ListenSessionProcedureId, listen_session_navigate_type);
            return Ok(listenSession);
        }
        [HttpPost("{BookingId}/deposit")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> PayDepositToBooking([FromRoute] int BookingId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _bookingService.ValidateBookingAccountAsync(BookingId, accountId);
            //if (!isValid)
            //{
            //    throw new UnauthorizedAccessException("You are not authorized to pay deposit for this booking.");
            //}
            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-deposit-payment-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking deposit payment process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }
        [HttpPost("{BookingId}/pay-the-rest")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> PayTheRestToBooking([FromRoute] int BookingId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            //var isValid = await _bookingService.ValidateBookingAccountAsync(BookingId, accountId);
            //if (!isValid)
            //{
            //    throw new UnauthorizedAccessException("You are not authorized to pay deposit for this booking.");
            //}
            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "BookingId", BookingId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "booking-pay-the-rest-payment-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate booking pay the rest payment process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }
    }
}
