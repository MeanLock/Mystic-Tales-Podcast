using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.BackgroundSoundTrack;
using PodcastService.BusinessLogic.Enums.App;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.Infrastructure.Services.Kafka;

namespace PodcastService.API.Controllers.MiscControllers
{
    [Route("api/misc/background-sound-tracks")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class BackgroundSoundTrackController : ControllerBase
    {
        private readonly ILogger<BackgroundSoundTrackController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly PodcastBackgroundSoundTrackService _backgroundSoundTrackService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        public BackgroundSoundTrackController(ILogger<BackgroundSoundTrackController> logger, KafkaProducerService kafkaProducerService, IMessagingService messagingService, PodcastBackgroundSoundTrackService backgroundSoundTrackService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper)
        {
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _backgroundSoundTrackService = backgroundSoundTrackService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
        }


        // /api/podcast-service/api/misc/background-sound-tracks/get-file-url/{**FileKey}
        [HttpGet("get-file-url/{**FileKey}")]
        public async Task<IActionResult> GetFileUrl(string FileKey)
        {
            // Determine access level dựa trên user auth status
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var requiredLevel = isAuthenticated ? FileAccessLevelEnum.RequiresAuth : FileAccessLevelEnum.Public;

            var validation = FileAccessValidator.ValidateFileAccess(FileKey, requiredLevel);

            if (!validation.IsValid)
            {
                return StatusCode(403, new { error = validation.ErrorMessage });
            }


            var url = await _fileIOHelper.GeneratePresignedUrlAsync(FileKey);
            return Ok(new { fileUrl = url });
        }

        // /api/podcast-service/api/misc/background-sound-tracks
        [HttpGet("")]
        [Authorize(Policy = "BasicAccess")]
        public async Task<IActionResult> GetBackgroundSoundTracks()
        {
            var soundTracks = await _backgroundSoundTrackService.GetBackgroundSoundTrackAsync();

            return Ok(new
            {
                BackgroundSoundTrackList = soundTracks
            });
        }

        // /api/podcast-service/api/misc/background-sound-tracks
        [HttpPost("")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> CreateBackgroundSoundTrack([FromForm] BackgroundSoundTrackCreateRequestDTO request)
        {
            var backgroundSoundTrackCreateInfo = JsonConvert.DeserializeObject<BackgroundSoundTrackCreateInfoDTO>(request.BackgroundSoundTrackCreateInfo);

            string mainImageFileKey = null;
            if (request.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastBackgroundSoundTrack.mainImageFileKey", request.MainImageFile.FileName, request.MainImageFile.Length, request.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }


                string newMainImageFileName = $"{Guid.NewGuid()}_{request.MainImageFile.FileName}";
                using (var stream = request.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }

                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH, newMainImageFileName);

            }
            string audioFileKey = null;
            if (request.AudioFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastBackgroundSoundTrack.audioFileKey", request.AudioFile.FileName, request.AudioFile.Length, request.AudioFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }
                string newAudioFileName = $"{Guid.NewGuid()}_{request.AudioFile.FileName}";
                using (var stream = request.AudioFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH,
                                        newAudioFileName
                                    );
                }
                audioFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH, newAudioFileName);
            }

            JObject requestData = JObject.FromObject(backgroundSoundTrackCreateInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["AudioFileKey"] = audioFileKey;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "background-sound-track-creation-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/misc/background-sound-tracks/{PodcastBackgroundSoundTrackId}
        [HttpPut("{PodcastBackgroundSoundTrackId}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> UpdateBackgroundSoundTrack([FromForm] BackgroundSoundTrackUpdateRequestDTO request, Guid PodcastBackgroundSoundTrackId)
        {
            var backgroundSoundTrackUpdateInfo = JsonConvert.DeserializeObject<BackgroundSoundTrackUpdateInfoDTO>(request.BackgroundSoundTrackUpdateInfo);

            string mainImageFileKey = null;
            if (request.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastBackgroundSoundTrack.mainImageFileKey", request.MainImageFile.FileName, request.MainImageFile.Length, request.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }

                string newMainImageFileName = $"{Guid.NewGuid()}_{request.MainImageFile.FileName}";
                using (var stream = request.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }

                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH, newMainImageFileName);

            }
            string audioFileKey = null;
            if (request.AudioFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastBackgroundSoundTrack.audioFileKey", request.AudioFile.FileName, request.AudioFile.Length, request.AudioFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }
                string newAudioFileName = $"{Guid.NewGuid()}_{request.AudioFile.FileName}";
                using (var stream = request.AudioFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH,
                                        newAudioFileName
                                    );
                }
                audioFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_TEMP_FILE_PATH, newAudioFileName);
            }

            JObject requestData = JObject.FromObject(backgroundSoundTrackUpdateInfo);
            requestData["PodcastBackgroundSoundTrackId"] = PodcastBackgroundSoundTrackId;
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["AudioFileKey"] = audioFileKey;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "background-sound-track-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/misc/background-sound-tracks/{PodcastBackgroundSoundTrackId}
        [HttpDelete("{PodcastBackgroundSoundTrackId}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> DeleteBackgroundSoundTrack(Guid PodcastBackgroundSoundTrackId)
        {
            var requestData = new JObject
            {
                ["PodcastBackgroundSoundTrackId"] = PodcastBackgroundSoundTrackId
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "background-sound-track-deletion-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
