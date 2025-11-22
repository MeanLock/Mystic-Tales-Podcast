using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.AudioTuning;
using PodcastService.BusinessLogic.DTOs.Auth;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.Enums.Account;
using PodcastService.BusinessLogic.Enums.App;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.Common.AppConfigurations.Media.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.Infrastructure.Helpers.AudioHelpers;
using PodcastService.Infrastructure.Models.Audio.Hls;
using PodcastService.Infrastructure.Models.Audio.Transcription;
using PodcastService.Infrastructure.Services.Audio.Hls;
using PodcastService.Infrastructure.Services.Audio.Transcription;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.Infrastructure.Services.Redis;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/episodes")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class EpisodeController : ControllerBase
    {
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;

        // private readonly PodcastChannelService _podcastChannelService;
        private readonly PodcastEpisodeService _podcastEpisodeService;
        private readonly RedisInstanceCacheService _redisInstanceCacheService;
        private readonly RedisSharedCacheService _redisSharedCacheService;
        private readonly AudioTranscriptionApiService _audioTranscriptionApiService;
        private readonly AppDbContext _appDbContext;
        private readonly IGenericRepository<PodcastEpisode> _podcastEpisodeGenericRepository;
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;
        private readonly IMediaTypeConfig _mediaTypeConfig;
        private readonly AudioFormatDetectorHelper _formatDetector;
        private readonly JwtHelper _jwtHelper;

        public EpisodeController(KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper, RedisInstanceCacheService redisInstanceCacheService, RedisSharedCacheService redisSharedCacheService, PodcastEpisodeService podcastEpisodeService, AudioTranscriptionApiService audioTranscriptionApiService, AppDbContext appDbContext, IGenericRepository<PodcastEpisode> podcastEpisodeGenericRepository, FFMpegCoreHlsService ffMpegCoreHlsService, IMediaTypeConfig mediaTypeConfig, JwtHelper jwtHelper)
        {
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _redisInstanceCacheService = redisInstanceCacheService;
            _redisSharedCacheService = redisSharedCacheService;
            _podcastEpisodeService = podcastEpisodeService;
            _audioTranscriptionApiService = audioTranscriptionApiService;
            _appDbContext = appDbContext;
            _podcastEpisodeGenericRepository = podcastEpisodeGenericRepository;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
            _mediaTypeConfig = mediaTypeConfig;
            _formatDetector = new AudioFormatDetectorHelper();
            _jwtHelper = jwtHelper;
        }

        #region Sample coding format must be followed
        #endregion

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}
        [HttpGet("{PodcastEpisodeId}")]
        public async Task<IActionResult> GetEpisodeById(Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var episode = await _podcastEpisodeService.GetEpisodeByIdAsync(PodcastEpisodeId, account?.RoleId);

            return Ok(new
            {
                Episode = episode
            });
        }

        // /api/podcast-service/api/episodes/me/{PodcastEpisodeId}
        [HttpGet("me/{PodcastEpisodeId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetMyEpisodeById(Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var episode = await _podcastEpisodeService.GetEpisodeByIdForPodcasterAsync(PodcastEpisodeId, account.Id);

            return Ok(new
            {
                Episode = episode
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/save/{IsSave}
        [HttpPost("{PodcastEpisodeId}/save/{IsSave}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> SaveOrUnsaveEpisodeById(Guid PodcastEpisodeId, bool IsSave)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var flowName = IsSave ? "episode-save-flow" : "episode-unsave-flow";
            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                ["AccountId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, flowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes
        [HttpPost("")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreateEpisode(EpisodeCreateRequestDTO episodeCreateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var episodeCreateInfo = JsonConvert.DeserializeObject<EpisodeCreateInfoDTO>(episodeCreateRequestDTO.EpisodeCreateInfo);

            string mainImageFileKey = null;
            if (episodeCreateRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastEpisode.mainImageFileKey", episodeCreateRequestDTO.MainImageFile.FileName, episodeCreateRequestDTO.MainImageFile.Length, episodeCreateRequestDTO.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }


                string newMainImageFileName = $"{Guid.NewGuid()}_{episodeCreateRequestDTO.MainImageFile.FileName}";
                using (var stream = episodeCreateRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }

                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH, newMainImageFileName);

            }
            JObject requestData = JObject.FromObject(episodeCreateInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["PodcasterId"] = account.Id; // lấy PodcasterId từ account đăng nhập hiện tại chứ không phải từ DTO

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-creation-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}
        [HttpPut("{PodcastEpisodeId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> UpdateEpisode(Guid PodcastEpisodeId, EpisodeUpdateRequestDTO episodeUpdateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var episodeUpdateInfo = JsonConvert.DeserializeObject<EpisodeUpdateInfoDTO>(episodeUpdateRequestDTO.EpisodeUpdateInfo);

            string mainImageFileKey = null;
            if (episodeUpdateRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastEpisode.mainImageFileKey", episodeUpdateRequestDTO.MainImageFile.FileName, episodeUpdateRequestDTO.MainImageFile.Length, episodeUpdateRequestDTO.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }


                string newMainImageFileName = $"{Guid.NewGuid()}_{episodeUpdateRequestDTO.MainImageFile.FileName}";
                using (var stream = episodeUpdateRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }

                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH, newMainImageFileName);

            }

            JObject requestData = JObject.FromObject(episodeUpdateInfo);
            requestData["PodcastEpisodeId"] = PodcastEpisodeId;
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["PodcasterId"] = account.Id;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}
        [HttpDelete("{PodcastEpisodeId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> DeleteEpisodeById(Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                // ["PodcasterId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-deletion-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes/podcast-episode-license-types
        [HttpGet("podcast-episode-license-types")]
        public async Task<IActionResult> GetPodcastEpisodeLicenseTypes()
        {
            var licenseTypes = await _podcastEpisodeService.GetPodcastEpisodeLicenseTypesAsync();

            return Ok(new
            {
                PodcastEpisodeLicenseTypeList = licenseTypes
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/licenses
        [HttpGet("{PodcastEpisodeId}/licenses")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess.Customer.PodcasterAccess")]
        public async Task<IActionResult> GetEpisodeLicensesById(Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var episodeLicenses = await _podcastEpisodeService.GetEpisodeLicensesByIdAsync(PodcastEpisodeId, account);

            return Ok(new
            {
                PodcastEpisodeLicenseList = episodeLicenses
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/licenses
        [HttpPost("{PodcastEpisodeId}/licenses")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> UploadEpisodeLicenseFile(Guid PodcastEpisodeId, List<IFormFile> LicenseDocumentFiles)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
            List<string> LicenseDocumentFileKeys = new List<string>();
            foreach (var file in LicenseDocumentFiles)
            {
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastEpisodeLicense.licenseDocumentFileKey", file.FileName, file.Length, file.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }

                string newLicenseFileName = $"{Guid.NewGuid()}_{file.FileName}";
                using (var stream = file.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH,
                                        newLicenseFileName
                                    );
                }

                string licenseFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH, newLicenseFileName);
                LicenseDocumentFileKeys.Add(licenseFileKey);
            }


            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                ["LicenseDocumentFileKeys"] = JArray.FromObject(LicenseDocumentFileKeys),
                ["PodcasterId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-licenses-upload-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/licenses/{PodcastEpisodeLicenseId}
        [HttpDelete("{PodcastEpisodeId}/licenses/{PodcastEpisodeLicenseId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> DeleteEpisodeLicenseFile(Guid PodcastEpisodeId, Guid PodcastEpisodeLicenseId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JArray podcastEpisodeLicenseIds = new JArray
            {
                PodcastEpisodeLicenseId
            };

            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                ["PodcastEpisodeLicenseIds"] = podcastEpisodeLicenseIds,
                ["PodcasterId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-licenses-deletion-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }


        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/audio
        [HttpPut("{PodcastEpisodeId}/audio")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> UploadEpisodeAudioFile(Guid PodcastEpisodeId, IFormFile AudioFile)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            // Validate and process the audio file
            var isValidFile = _fileValidationConfig.IsValidFile("PodcastEpisode.audioFileKey", AudioFile.FileName, AudioFile.Length, AudioFile.ContentType);
            if (!isValidFile)
            {
                return BadRequest("Invalid audio file.");
            }
            string newAudioFileName = $"{Guid.NewGuid()}_{AudioFile.FileName}";
            int audioLengthSeconds;
            using (var stream = AudioFile.OpenReadStream())
            {
                audioLengthSeconds = (int)await FFmpegCoreHelper.GetAudioDurationSecondsFromStreamAsync(stream);
                await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                    stream,
                                    _filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH,
                                    newAudioFileName
                                );
            }

            string audioFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_EPISODE_TEMP_FILE_PATH, newAudioFileName);

            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                ["AudioFileKey"] = audioFileKey,
                ["PodcasterId"] = account.Id,
                ["AudioFileSize"] = AudioFile.Length / (1024.0 * 1024.0),
                ["AudioLength"] = audioLengthSeconds
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-audio-submission-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/publish-review/create-request
        [HttpPost("{PodcastEpisodeId}/publish-review/create-request")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> CreateEpisodePublishReviewRequest(Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                ["PodcasterId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-moderation-domain", requestData, null, "episode-publish-request-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/publish-review/discard-request
        [HttpPost("{PodcastEpisodeId}/publish-review/discard-request")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> DiscardEpisodePublishReviewRequest(Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                ["PodcasterId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-moderation-domain", requestData, null, "episode-publish-review-session-discard-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/publish/{IsPublish}
        [HttpPut("{PodcastEpisodeId}/publish/{IsPublish}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> PublishOrUnpublishEpisodeById(Guid PodcastEpisodeId, bool IsPublish)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var flowName = IsPublish ? "episode-publish-flow" : "episode-unpublish-flow";
            JObject requestData = new JObject
            {
                ["PodcastEpisodeId"] = PodcastEpisodeId,
                ["PodcasterId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, flowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/listen
        [HttpPost("{PodcastEpisodeId}/listen")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> RecordEpisodeListen(Guid PodcastEpisodeId, [FromBody] EpisodeListenRequestDTO listenRequestDTO, [FromQuery] string? Token = null)
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


            var episodeListenResponse = await _podcastEpisodeService.GetEpisodeListenAsync(PodcastEpisodeId, account.Id, listenRequestDTO.CurrentPodcastSubscriptionRegistrationBenefitList, deviceInfo, Token);

            return Ok(new
            {
                ListenSession = episodeListenResponse
            });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/hls-encryption-key/{KeyId}
        [HttpGet("{PodcastEpisodeId}/hls-encryption-key/{KeyId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetEpisodeHlsEncryptionKeyFileUrl(Guid PodcastEpisodeId, Guid KeyId, [FromQuery] string? Token = null)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var encryptionKeyBytes = await _podcastEpisodeService.GetEpisodeHlsEncryptionKeyFileAsync(PodcastEpisodeId, KeyId, Token);

            Response.Headers.CacheControl = "no-store";
            return File(encryptionKeyBytes, "application/octet-stream", enableRangeProcessing: false);
        }

        // /api/podcast-service/api/episodes/hls-playlist/get-file-data/{**FileKey}
        [HttpGet("hls-playlist/get-file-data/{**FileKey}")]
        // [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetHlsPlaylistFileUrl(string FileKey)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // Validate file key phải là HLS playlist
            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.PodcastEpisodeHlsPlaylist)
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

        // /api/podcast-service/api/episodes/hls-segment/get-file-data/{**FileKey}
        [HttpGet("hls-segment/get-file-data/{**FileKey}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetHlsSegmentFileUrl(string FileKey)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // Validate file key phải là HLS segment
            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.PodcastEpisodeHlsSegment)
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

        // /api/podcast-service/api/episodes/audio/get-file-url/{**FileKey}
        [HttpGet("audio/get-file-url/{**FileKey}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess.Customer.PodcasterAccess")]
        public async Task<IActionResult> GetEpisodeAudioFileUrl(string FileKey)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.EpisodeRawAudio)
            {
                return StatusCode(403, new
                {
                    error = "Invalid file key: Must be an Episode License Document file",
                    actualCategory = category.ToString()
                });
            }
            
            var url = await _fileIOHelper.GeneratePresignedUrlAsync(FileKey);

            if (account.RoleId == (int)RoleEnum.Customer)
            {
                var isOwnedByPodcaster = await _podcastEpisodeService.IsAudioFileOwnedByPodcasterAsync(FileKey, account.Id);
                if (!isOwnedByPodcaster)
                {
                    return StatusCode(403, new
                    {
                        error = "Access denied: You do not have permission to access this audio file"
                    });
                }
            }

            return Ok(new { FileUrl = url });
        }

        // /api/podcast-service/api/episodes/license-document/get-file-url/{**FileKey}
        [HttpGet("license-document/get-file-url/{**FileKey}")]
        public async Task<IActionResult> GetLicenseDocumentFileUrl(string FileKey)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // Validate file key phải là HLS segment
            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.EpisodeLicenseDocument)
            {
                return StatusCode(403, new
                {
                    error = "Invalid file key: Must be an Episode License Document file",
                    actualCategory = category.ToString()
                });
            }
            var url = await _fileIOHelper.GeneratePresignedUrlAsync(FileKey);

            return Ok(new { FileUrl = url });
        }

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/audio-tuning/general
        [HttpPost("{PodcastEpisodeId}/audio-tuning/general")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetEpisodeAudioGeneralTuningSettings(Guid PodcastEpisodeId, [FromForm] GeneralAudioTuningRequestDTO generalAudioTuningRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            Stream tunedAudio;
            GeneralTuningProfileRequestInfo generalTuningProfileRequestInfo = JsonConvert.DeserializeObject<GeneralTuningProfileRequestInfo>(generalAudioTuningRequestDTO.GeneralTuningProfileRequestInfo);

            var inputStream = generalAudioTuningRequestDTO.AudioFile.OpenReadStream();

            tunedAudio = await _podcastEpisodeService.GetEpisodeAudioGeneralTuningSettingsAsync(
                generalTuningProfileRequestInfo, inputStream, PodcastEpisodeId, account.Id);

            if (tunedAudio == null)
            {
                return BadRequest("Tuning process returned null stream");
            }

            var formatInfo = _formatDetector.DetectFormatFromStream(tunedAudio);

            if (tunedAudio.CanSeek)
            {
                tunedAudio.Position = 0;
            }

            Response.Headers.CacheControl = "no-store";

            return File(tunedAudio, formatInfo.MimeType, enableRangeProcessing: false);

        }

        // /api/podcast-service/api/episodes/listen-sessions/podcast-episode-listen-history
        [HttpGet("listen-sessions/podcast-episode-listen-history")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetPodcastEpisodeListenHistory([FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var listenHistory = await _podcastEpisodeService.GetPodcastEpisodeListenHistoryAsync(account.Id);

            return Ok(new
            {
                ListenHistory = listenHistory
            });
        }

        // /api/podcast-service/api/episodes/listen-sessions/latest
        [HttpGet("listen-sessions/latest")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetLatestPodcastEpisodeListenSession()
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

            var listenSession = await _podcastEpisodeService.GetLatestPodcastEpisodeListenSessionAsync(account.Id, deviceInfo);

            return Ok(new
            {
                ListenSession = listenSession
            });
        }

        // /api/podcast-service/api/episodes/listen-sessions/{PodcastEpisodeListenSessionId}/last-duration-seconds/{LastListenDurationSeconds}
        [HttpPut("listen-sessions/{PodcastEpisodeListenSessionId}/last-duration-seconds/{LastListenDurationSeconds}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> UpdatePodcastEpisodeListenSessionLastDurationSeconds(Guid PodcastEpisodeListenSessionId, int LastListenDurationSeconds, [FromBody] EpisodeListenLastDurationSecondsUpdateRequestDTO listenRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["PodcastEpisodeListenSessionId"] = PodcastEpisodeListenSessionId,
                ["ListenerId"] = account.Id,
                ["LastListenDurationSeconds"] = LastListenDurationSeconds,
                ["CurrentPodcastSubscriptionRegistrationBenefitList"] = JArray.FromObject(listenRequestDTO.CurrentPodcastSubscriptionRegistrationBenefitList)
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-listen-session-duration-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }




        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost("audio-duration-seconds-test")]
        public async Task<IActionResult> TestAudioDurationSeconds(IFormFile AudioFile)
        {
            var durationSeconds = 0.0;
            using (var stream = AudioFile.OpenReadStream())
            {
                durationSeconds = await FFmpegCoreHelper.GetAudioDurationSecondsFromStreamAsync(stream);
            }
            return Ok(new
            {
                DurationSeconds = durationSeconds
            });
        }

        [HttpPost("audio-transcription-test")]
        public async Task<IActionResult> TestAudioTranscription(IFormFile AudioFile)
        {
            var transcriptionText = new AudioTranscriptionApiResult();
            using (var stream = AudioFile.OpenReadStream())
            {
                transcriptionText = await _audioTranscriptionApiService.TranscribeAudioAsync(stream);
            }
            return Ok(transcriptionText);
        }

        [HttpPost("test-update-audiofingerprint")]
        public async Task<IActionResult> TestUpdateAudioFingerprint([FromForm] string AudioFingerprintData, [FromForm] Guid PodcastEpisodeId)
        {
            var audioFingerprint = System.Text.Encoding.UTF8.GetBytes(AudioFingerprintData);

            var episode = await _podcastEpisodeGenericRepository.FindByIdAsync(PodcastEpisodeId);
            episode.AudioFingerPrint = audioFingerprint;

            await _podcastEpisodeGenericRepository.UpdateAsync(episode.Id, episode);

            return Ok();
        }

        [HttpPost("upload-audio-ffmpegCore")] // upload-audio return file key
        public async Task<IActionResult> UploadAudioFFmpegCore(IFormFile file,
            [FromForm] string folderPath, [FromForm] string fileName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                await _fileIOHelper.UploadBinaryFileAsync(fileData, folderPath, fileName, file.ContentType);

                var stream = await _fileIOHelper.GetFileStreamAsync(FilePathHelper.CombinePaths(folderPath, fileName));
                if (stream == null)
                {
                    return BadRequest(new { error = "Failed to retrieve uploaded file stream" });
                }

                HlsProcessingResult hlsResult = await _ffMpegCoreHlsService.ProcessAudioToHlsAsync(stream);
                if (hlsResult.Success == false)
                {
                    return BadRequest(new { error = hlsResult.ErrorMessage });
                }

                // xoá hết các file cũ trong thư mục playlist (nếu có)
                await _fileIOHelper.DeleteFolderAsync(FilePathHelper.CombinePaths(folderPath, "playlist"));

                foreach (var segment in hlsResult.GeneratedFiles)
                {
                    // var segmentData = await _fileIOHelper.GetFileBytesAsync(segment.FilePath);
                    // if (segmentData == null)
                    // {
                    //     return BadRequest(new { error = $"Failed to read segment file: {segment.FilePath}" });
                    // }
                    var segmentData = segment.FileContent;

                    await _fileIOHelper.UploadBinaryFileAsync(segmentData, FilePathHelper.CombinePaths(folderPath, "playlist"), segment.FileName);
                }
                await _fileIOHelper.UploadBinaryFileAsync(hlsResult.EncryptionKeyFile.FileContent, FilePathHelper.CombinePaths(folderPath, "playlist"), hlsResult.EncryptionKeyFile.FileName);

                string playlistUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(folderPath, "playlist", "playlist.m3u8"), 20);
                string playlistFileKey = await _fileIOHelper.GetFullFileKeyAsync(FilePathHelper.CombinePaths(folderPath, "playlist", "playlist.m3u8"));
                // string ePlaylistUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistUrl));
                string ePlaylistFileKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistFileKey));



                return Ok(new
                {
                    message = "Binary file uploaded successfully",
                    fileName,
                    size = fileData.Length,
                    contentType = file.ContentType,
                    fileKey = FilePathHelper.NormalizeFilePath(FilePathHelper.CombinePaths(folderPath, $"{fileName}{_mediaTypeConfig.GetExtensionFromMimeType(file.ContentType)}")),
                    playlistFileKey,
                    folderPath = FilePathHelper.CombinePaths(folderPath, "playlist"),
                    // playlistUrl,
                    ePlaylistFileKey
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }




    }
}
