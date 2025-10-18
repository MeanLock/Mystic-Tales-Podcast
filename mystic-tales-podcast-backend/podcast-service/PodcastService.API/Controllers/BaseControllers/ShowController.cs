using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.Infrastructure.Services.Redis;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/shows")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class ShowController : ControllerBase
    {
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;

        // private readonly PodcastChannelService _podcastChannelService;
        private readonly PodcastShowService _podcastShowService;
        private readonly RedisInstanceCacheService _redisInstanceCacheService;
        private readonly RedisSharedCacheService _redisSharedCacheService;

        public ShowController(KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper, RedisInstanceCacheService redisInstanceCacheService, RedisSharedCacheService redisSharedCacheService, PodcastChannelService podcastChannelService, PodcastShowService podcastShowService)
        {
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _redisInstanceCacheService = redisInstanceCacheService;
            _redisSharedCacheService = redisSharedCacheService;
            _podcastShowService = podcastShowService;
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

        // /api/podcast-service/api/shows
        [HttpGet("")]
        public async Task<IActionResult> GetShows([FromQuery] int? PodcasterId, [FromQuery] string? SearchKeyword, [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var shows = await _podcastShowService.GetShows(account?.RoleId);

            return Ok(new
            {
                ShowList = shows
            });
        }

        // /api/podcast-service/api/shows
        [HttpPost("")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreateShow(ShowCreateRequestDTO showCreateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var showCreateInfo = JsonConvert.DeserializeObject<ShowCreateInfoDTO>(showCreateRequestDTO.ShowCreateInfo);

            string mainImageFileKey = null;
            if (showCreateRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastShow.mainImageFileKey", showCreateRequestDTO.MainImageFile.FileName, showCreateRequestDTO.MainImageFile.Length, showCreateRequestDTO.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }


                string newMainImageFileName = $"{Guid.NewGuid()}_{showCreateRequestDTO.MainImageFile.FileName}";
                using (var stream = showCreateRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_SHOW_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }

                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_SHOW_TEMP_FILE_PATH, newMainImageFileName);

            }

            JObject requestData = JObject.FromObject(showCreateInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["PodcasterId"] = account.Id; // lấy PodcasterId từ account đăng nhập hiện tại chứ không phải từ DTO

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "show-creation-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/shows/me
        [HttpGet("me")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetMyShows()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var shows = await _podcastShowService.GetShowsByPodcasterIdAsync(account.Id);

            return Ok(new
            {
                ShowList = shows
            });
        }

        // /api/podcast-service/api/shows/{PodcastShowId}
        [HttpGet("{PodcastShowId}")]
        public async Task<IActionResult> GetShowById(Guid PodcastShowId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var show = await _podcastShowService.GetShowByIdAsync(PodcastShowId, account?.RoleId);

            return Ok(new
            {
                Show = show
            });
        }

        // /api/podcast-service/api/shows/{PodcastShowId}
        [HttpPut("{PodcastShowId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> UpdateShowById(Guid PodcastShowId, ShowUpdateRequestDTO showUpdateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var showUpdateInfo = JsonConvert.DeserializeObject<ShowUpdateInfoDTO>(showUpdateRequestDTO.ShowUpdateInfo);

            string mainImageFileKey = null;
            if (showUpdateRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastShow.mainImageFileKey", showUpdateRequestDTO.MainImageFile.FileName, showUpdateRequestDTO.MainImageFile.Length, showUpdateRequestDTO.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }
                string newMainImageFileName = $"{Guid.NewGuid()}_{showUpdateRequestDTO.MainImageFile.FileName}";
                using (var stream = showUpdateRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_SHOW_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }
                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_SHOW_TEMP_FILE_PATH, newMainImageFileName);
            }

            JObject requestData = JObject.FromObject(showUpdateInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["PodcastShowId"] = PodcastShowId;
            requestData["PodcasterId"] = account.Id;

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "show-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }


        // /api/podcast-service/api/shows/{PodcastShowId}/trailer-audio
        [HttpPut("{PodcastShowId}/trailer-audio")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> UploadOrUpdateShowTrailerAudioById(Guid PodcastShowId, IFormFile TrailerAudioFile)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
            var isValidFile = _fileValidationConfig.IsValidFile("PodcastShow.trailerAudioFileKey", TrailerAudioFile.FileName, TrailerAudioFile.Length, TrailerAudioFile.ContentType);
            if (!isValidFile)
            {
                return BadRequest("Invalid upload file.");
            }
            string newTrailerAudioFileName = $"{Guid.NewGuid()}_{TrailerAudioFile.FileName}";
            using (var stream = TrailerAudioFile.OpenReadStream())
            {
                await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                    stream,
                                    _filePathConfig.PODCAST_SHOW_TEMP_FILE_PATH,
                                    newTrailerAudioFileName
                                );
            }
            string trailerAudioFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_SHOW_TEMP_FILE_PATH, newTrailerAudioFileName);

            JObject requestData = new JObject
            {
                ["PodcastShowId"] = PodcastShowId,
                ["PodcasterId"] = account.Id,
                ["TrailerAudioFileKey"] = trailerAudioFileKey
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "show-trailer-audio-submission-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/shows/{PodcastShowId}/publish/{IsPublish}
        [HttpPut("{PodcastShowId}/publish/{IsPublish}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> PublishOrUnpublishShowById(Guid PodcastShowId, bool IsPublish, ShowPublishRequestDTO showPublishRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (IsPublish == true && account.ViolationLevel >= 0)
            {
                return StatusCode(403, "Your account has violation level that is not allowed to show channel.");
            }
            
            var flowName = IsPublish ? "show-publish-flow" : "show-unpublish-flow";
            JObject requestData = new JObject
            {
                ["PodcastShowId"] = PodcastShowId,
                ["PodcasterId"] = account.Id,
                ["ShowPublishInfo"] = JObject.FromObject(showPublishRequestDTO.ShowPublishInfo)
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, flowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }



    }
}
