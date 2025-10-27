using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.AudioTuning;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.Enums.App;
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

        public EpisodeController(KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper, RedisInstanceCacheService redisInstanceCacheService, RedisSharedCacheService redisSharedCacheService, PodcastEpisodeService podcastEpisodeService, AudioTranscriptionApiService audioTranscriptionApiService, AppDbContext appDbContext, IGenericRepository<PodcastEpisode> podcastEpisodeGenericRepository, FFMpegCoreHlsService ffMpegCoreHlsService, IMediaTypeConfig mediaTypeConfig)
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
        }

        #region Sample coding format must be followed
        // // /api/podcast-service/api/channels
        // [HttpGet("")]
        // public async Task<IActionResult> GetChannels([FromQuery] int? PodcasterId, [FromQuery] string? SearchKeyword, [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     var channels = await _podcastChannelService.GetChannels(account?.RoleId);

        //     return Ok(new
        //     {
        //         ChannelList = channels
        //     });
        // }

        // // /api/podcast-service/api/channels
        // [HttpPost("")]
        // [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        // public async Task<IActionResult> CreateChannel(ChannelCreateRequestDTO channelCreateRequestDTO)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     var channelCreateInfo = JsonConvert.DeserializeObject<ChannelCreateInfoDTO>(channelCreateRequestDTO.ChannelCreateInfo);

        //     string mainImageFileKey = null;
        //     if (channelCreateRequestDTO.MainImageFile != null)
        //     {
        //         // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
        //         var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.mainImageFileKey", channelCreateRequestDTO.MainImageFile.FileName, channelCreateRequestDTO.MainImageFile.Length, channelCreateRequestDTO.MainImageFile.ContentType);
        //         if (!isValidFile)
        //         {
        //             return BadRequest("Invalid upload file.");
        //         }


        //         string newMainImageFileName = $"{Guid.NewGuid()}_{channelCreateRequestDTO.MainImageFile.FileName}";
        //         using (var stream = channelCreateRequestDTO.MainImageFile.OpenReadStream())
        //         {
        //             await _fileIOHelper.UploadBinaryFileWithStreamAsync(
        //                                 stream,
        //                                 _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
        //                                 newMainImageFileName
        //                             );
        //         }

        //         mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newMainImageFileName);

        //     }

        //     string backgroundImageFileKey = null;
        //     if (channelCreateRequestDTO.BackgroundImageFile != null)
        //     {
        //         // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
        //         var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.backgroundImageFileKey", channelCreateRequestDTO.BackgroundImageFile.FileName, channelCreateRequestDTO.BackgroundImageFile.Length, channelCreateRequestDTO.BackgroundImageFile.ContentType);
        //         if (!isValidFile)
        //         {
        //             return BadRequest("Invalid upload file.");
        //         }
        //         string newBackgroundImageFileName = $"{Guid.NewGuid()}_{channelCreateRequestDTO.BackgroundImageFile.FileName}";
        //         using (var stream = channelCreateRequestDTO.BackgroundImageFile.OpenReadStream())
        //         {
        //             await _fileIOHelper.UploadBinaryFileWithStreamAsync(
        //                                 stream,
        //                                 _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
        //                                 newBackgroundImageFileName
        //                             );
        //         }
        //         backgroundImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newBackgroundImageFileName);
        //     }

        //     JObject requestData = JObject.FromObject(channelCreateInfo);
        //     requestData["MainImageFileKey"] = mainImageFileKey;
        //     requestData["BackgroundImageFileKey"] = backgroundImageFileKey;
        //     requestData["PodcasterId"] = account.Id; // lấy PodcasterId từ account đăng nhập hiện tại chứ không phải từ DTO


        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "channel-creation-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/podcast-service/api/channels/me
        // [HttpGet("me")]
        // [Authorize(Policy = "Customer.PodcasterAccess")]
        // public async Task<IActionResult> GetMyChannel()
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

        //     var channels = await _podcastChannelService.GetChannelByPodcasterIdAsync(account.Id);

        //     return Ok(new
        //     {
        //         ChannelList = channels
        //     });
        // }

        // // /api/podcast-service/api/channels/{PodcastChannelId}
        // [HttpGet("{PodcastChannelId}")]
        // public async Task<IActionResult> GetChannelById(Guid PodcastChannelId)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

        //     var channel = await _podcastChannelService.GetChannelByIdAsync(PodcastChannelId, account?.RoleId);

        //     return Ok(new
        //     {
        //         Channel = channel
        //     });
        // }

        // // /api/podcast-service/api/channels/{PodcastChannelId}
        // [HttpPut("{PodcastChannelId}")]
        // [Authorize(Policy = "Customer.PodcasterAccess")]
        // public async Task<IActionResult> UpdateChannelById(Guid PodcastChannelId, ChannelUpdateRequestDTO channelUpdateRequestDTO)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     var channelUpdateInfo = JsonConvert.DeserializeObject<ChannelUpdateInfoDTO>(channelUpdateRequestDTO.ChannelUpdateInfo);

        //     string mainImageFileKey = null;
        //     if (channelUpdateRequestDTO.MainImageFile != null)
        //     {
        //         // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
        //         var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.mainImageFileKey", channelUpdateRequestDTO.MainImageFile.FileName, channelUpdateRequestDTO.MainImageFile.Length, channelUpdateRequestDTO.MainImageFile.ContentType);
        //         if (!isValidFile)
        //         {
        //             return BadRequest("Invalid upload file.");
        //         }
        //         string newMainImageFileName = $"{Guid.NewGuid()}_{channelUpdateRequestDTO.MainImageFile.FileName}";
        //         using (var stream = channelUpdateRequestDTO.MainImageFile.OpenReadStream())
        //         {
        //             await _fileIOHelper.UploadBinaryFileWithStreamAsync(
        //                                 stream,
        //                                 _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
        //                                 newMainImageFileName
        //                             );
        //         }
        //         mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newMainImageFileName);
        //     }
        //     string backgroundImageFileKey = null;
        //     if (channelUpdateRequestDTO.BackgroundImageFile != null)
        //     {
        //         // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
        //         var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.backgroundImageFileKey", channelUpdateRequestDTO.BackgroundImageFile.FileName, channelUpdateRequestDTO.BackgroundImageFile.Length, channelUpdateRequestDTO.BackgroundImageFile.ContentType);
        //         if (!isValidFile)
        //         {
        //             return BadRequest("Invalid upload file.");
        //         }
        //         string newBackgroundImageFileName = $"{Guid.NewGuid()}_{channelUpdateRequestDTO.BackgroundImageFile.FileName}";
        //         using (var stream = channelUpdateRequestDTO.BackgroundImageFile.OpenReadStream())
        //         {
        //             await _fileIOHelper.UploadBinaryFileWithStreamAsync(
        //                                 stream,
        //                                 _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
        //                                 newBackgroundImageFileName
        //                             );
        //         }
        //         backgroundImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newBackgroundImageFileName);
        //     }
        //     JObject requestData = JObject.FromObject(channelUpdateInfo);
        //     requestData["MainImageFileKey"] = mainImageFileKey;
        //     requestData["BackgroundImageFileKey"] = backgroundImageFileKey;
        //     requestData["PodcastChannelId"] = PodcastChannelId;
        //     requestData["PodcasterId"] = account.Id;

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "channel-update-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/podcast-service/api/channels/{PodcastChannelId}
        // [HttpDelete("{PodcastChannelId}")]
        // [Authorize(Policy = "Customer.PodcasterAccess")]
        // public async Task<IActionResult> DeleteChannelById(ChannelDeleteRequestDTO channelDeleteRequestDTO, Guid PodcastChannelId)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

        //     JObject requestData = new JObject
        //     {
        //         ["PodcastChannelId"] = PodcastChannelId,
        //         ["PodcasterId"] = account.Id
        //     };

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "channel-deletion-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/podcast-service/api/channels/{PodcastChannelId}/publish/{IsPublish}
        // [HttpPut("{PodcastChannelId}/publish/{IsPublish}")]
        // [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        // public async Task<IActionResult> PublishOrUnpublishChannelById(Guid PodcastChannelId, bool IsPublish)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

        //     var flowName = IsPublish ? "channel-publish-flow" : "channel-unpublish-flow";
        //     JObject requestData = new JObject
        //     {
        //         ["PodcastChannelId"] = PodcastChannelId,
        //         ["PodcasterId"] = account.Id
        //     };

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, flowName);
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/podcast-service/api/channels/{PodcastShowId}/favorite/{IsFavorite}
        // [HttpPost("{PodcastChannelId}/favorite/{IsFavorite}")]
        // [Authorize(Policy = "Customer.BasicAccess")]
        // public async Task<IActionResult> FavoriteOrUnfavoriteChannelById(Guid PodcastChannelId, bool IsFavorite)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;


        //     var flowName = IsFavorite ? "channel-favorite-flow" : "channel-unfavorite-flow";
        //     JObject requestData = new JObject
        //     {
        //         ["PodcastChannelId"] = PodcastChannelId,
        //         ["AccountId"] = account.Id
        //     };

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, flowName);
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }
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
        [HttpGet("{PodcastEpisodeId}/listen")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> RecordEpisodeListen(Guid PodcastEpisodeId, [FromQuery] string? Token = null)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var episodeListenResponse = await _podcastEpisodeService.GetEpisodeListenAsync(PodcastEpisodeId, account.Id, Token);

            return Ok(episodeListenResponse);
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

            if (category != FileCategoryEnum.HlsPlaylist)
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

            if (category != FileCategoryEnum.HlsSegment)
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

        // /api/podcast-service/api/episodes/{PodcastEpisodeId}/audio-tuning/general
        [HttpPost("{PodcastEpisodeId}/audio-tuning/general")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetEpisodeAudioGeneralTuningSettings(Guid PodcastEpisodeId, [FromForm] GeneralAudioTuningRequestDTO generalAudioTuningRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            Stream tunedAudio;
            GeneralTuningProfileRequestInfo generalTuningProfileRequestInfo = JsonConvert.DeserializeObject<GeneralTuningProfileRequestInfo>(generalAudioTuningRequestDTO.GeneralTuningProfileRequestInfo);

            // using (var stream = generalAudioTuningRequestDTO.AudioFile.OpenReadStream())
            // {
            //     tunedAudio = await _podcastEpisodeService.GetEpisodeAudioGeneralTuningSettingsAsync(generalTuningProfileRequestInfo, stream, PodcastEpisodeId, account.Id);
            //     // ĐẢM BẢO STREAM HỢP LỆ
            //     if (tunedAudio == null)
            //     {
            //         return BadRequest("Tuning process returned null stream");
            //     }

            //     // RESET POSITION
            //     if (tunedAudio.CanSeek)
            //     {
            //         tunedAudio.Position = 0;
            //     }


            //     Response.Headers.CacheControl = "no-store";
            //     return File(tunedAudio, _formatDetector.DetectFormatFromStream(tunedAudio).MimeType, enableRangeProcessing: false);
            // }
            var inputStream = generalAudioTuningRequestDTO.AudioFile.OpenReadStream();

            tunedAudio = await _podcastEpisodeService.GetEpisodeAudioGeneralTuningSettingsAsync(
                generalTuningProfileRequestInfo, inputStream, PodcastEpisodeId, account.Id);

            if (tunedAudio == null)
            {
                return BadRequest("Tuning process returned null stream");
            }

            // ✅ Detect format TRƯỚC khi reset position
            var formatInfo = _formatDetector.DetectFormatFromStream(tunedAudio);

            // ✅ Reset position SAU khi detect
            if (tunedAudio.CanSeek)
            {
                tunedAudio.Position = 0;
            }

            Response.Headers.CacheControl = "no-store";

            // ✅ Disable range processing để tránh partial content
            return File(tunedAudio, formatInfo.MimeType, enableRangeProcessing: false);

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
