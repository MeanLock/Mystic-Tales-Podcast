using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.ReviewSession;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.Infrastructure.Helpers.AudioHelpers;
using PodcastService.Infrastructure.Models.Audio.Transcription;
using PodcastService.Infrastructure.Services.Audio.Transcription;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.Infrastructure.Services.Redis;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/review-sessions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class ReviewSessionController : ControllerBase
    {
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;

        // private readonly PodcastChannelService _podcastChannelService;
        private readonly ReviewSessionService _reviewSessionService;
        private readonly RedisInstanceCacheService _redisInstanceCacheService;
        private readonly RedisSharedCacheService _redisSharedCacheService;
        private readonly AudioTranscriptionApiService _audioTranscriptionApiService;
        private readonly AppDbContext _appDbContext;
        private readonly IGenericRepository<PodcastEpisode> _podcastEpisodeGenericRepository;

        public ReviewSessionController(KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper, RedisInstanceCacheService redisInstanceCacheService, RedisSharedCacheService redisSharedCacheService, ReviewSessionService reviewSessionService, AudioTranscriptionApiService audioTranscriptionApiService, AppDbContext appDbContext, IGenericRepository<PodcastEpisode> podcastEpisodeGenericRepository)
        {
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _redisInstanceCacheService = redisInstanceCacheService;
            _redisSharedCacheService = redisSharedCacheService;
            _reviewSessionService = reviewSessionService;
            _audioTranscriptionApiService = audioTranscriptionApiService;
            _appDbContext = appDbContext;
            _podcastEpisodeGenericRepository = podcastEpisodeGenericRepository;
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

        // /api/podcast-service/api/review-sessions/episode-publish
        [HttpGet("episode-publish")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetMyEpisodePublishReviewSessions([FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var publishReviewSessions = await _reviewSessionService.GetEpisodePublishReviewSessionsByPodcasterIdAsync(account);

            return Ok(new
            {
                ReviewSessionList = publishReviewSessions
            });
        }

        // /api/podcast-service/api/review-sessions/episode-publish/episodes/{PodcastEpisodeId}/current
        [HttpGet("episode-publish/episodes/{PodcastEpisodeId}/current")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess.Customer.PodcasterAccess")]
        public async Task<IActionResult> GetCurrentEpisodePublishReviewSessionByEpisodeId(Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var publishReviewSession = await _reviewSessionService.GetCurrentEpisodePublishReviewSessionByEpisodeIdAsync(PodcastEpisodeId, account);

            return Ok(new
            {
                ReviewSession = publishReviewSession
            });
        }

        // /api/podcast-service/api/review-sessions/episode-publish/{PodcastEpisodePublishReviewSessionId}
        [HttpGet("episode-publish/{PodcastEpisodePublishReviewSessionId}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetEpisodePublishReviewSessionById(int PodcastEpisodePublishReviewSessionId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var publishReviewSession = await _reviewSessionService.GetEpisodePublishReviewSessionByIdAsync(PodcastEpisodePublishReviewSessionId, account);

            return Ok(new
            {
                ReviewSession = publishReviewSession
            });
        }

        // /api/podcast-service/api/review-sessions/episode-publish/{PodcastEpisodePublishReviewSessionId}/edit-require
        [HttpPut("episode-publish/{PodcastEpisodePublishReviewSessionId}/edit-require")]
        [Authorize(Policy = "Staff.BasicAccess")]
        public async Task<IActionResult> EditRequireForEpisodePublishReviewSessionById(int PodcastEpisodePublishReviewSessionId, [FromBody] EpisodePublishReviewSessionUpdateRequestDTO episodePublishReviewSessionEditRequireRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["PodcastEpisodePublishReviewSessionId"] = PodcastEpisodePublishReviewSessionId,
                ["Note"] = episodePublishReviewSessionEditRequireRequestDTO.EpisodePublishReviewSessionUpdateInfo.Note,
                ["PodcastIllegalContentTypeIds"] = JArray.FromObject(episodePublishReviewSessionEditRequireRequestDTO.EpisodePublishReviewSessionUpdateInfo.PodcastIllegalContentTypeIds),
                ["StaffId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-moderation-domain", requestData, null, "episode-publish-review-session-edit-require-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        // /api/podcast-service/api/review-sessions/episode-publish/{PodcastEpisodePublishReviewSessionId}/accept/{IsAccepted}
        [HttpPut("episode-publish/{PodcastEpisodePublishReviewSessionId}/accept/{IsAccepted}")]
        [Authorize(Policy = "Staff.BasicAccess")]
        public async Task<IActionResult> AcceptOrRejectEpisodePublishReviewSessionById(int PodcastEpisodePublishReviewSessionId, bool IsAccepted, [FromBody] EpisodePublishReviewSessionUpdateRequestDTO episodePublishReviewSessionEditRequireRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var flowName = IsAccepted ? "episode-publish-review-session-acceptance-flow" : "episode-publish-review-session-rejection-flow";
            JObject requestData = new JObject
            {
                ["PodcastEpisodePublishReviewSessionId"] = PodcastEpisodePublishReviewSessionId,
                ["Note"] = episodePublishReviewSessionEditRequireRequestDTO.EpisodePublishReviewSessionUpdateInfo.Note,
                ["PodcastIllegalContentTypeIds"] = JArray.FromObject(episodePublishReviewSessionEditRequireRequestDTO.EpisodePublishReviewSessionUpdateInfo.PodcastIllegalContentTypeIds),
                ["StaffId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-moderation-domain", requestData, null, flowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

        

        
    }
}
