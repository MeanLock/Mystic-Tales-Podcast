using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.Infrastructure.Services.Redis;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/channels")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class ChannelController : ControllerBase
    {
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;

        private readonly PodcastChannelService _podcastChannelService;
        private readonly RedisInstanceCacheService _redisInstanceCacheService;
        private readonly RedisSharedCacheService _redisSharedCacheService;

        public ChannelController(KafkaProducerService kafkaProducerService, IMessagingService messagingService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper, RedisInstanceCacheService redisInstanceCacheService, RedisSharedCacheService redisSharedCacheService, PodcastChannelService podcastChannelService)
        {
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _redisInstanceCacheService = redisInstanceCacheService;
            _redisSharedCacheService = redisSharedCacheService;
            _podcastChannelService = podcastChannelService;
        }

        #region Sample coding format must be followed
        // [HttpGet("test-get-account-status")]
        // [Authorize(Policy = "Customer.NoViolationAccess")]
        // public async Task<IActionResult> TestGetAccountStatus()
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     return Ok(new
        //     {
        //         Account = account,
        //         Message = $"Hello, your account ID is {account.Id}, RoleId is {account.RoleId}, ViolationLevel is {account.ViolationLevel}, ViolationPoint is {account.ViolationPoint}, IsVerified is {account.IsVerified}, DeactivatedAt is {account.DeactivatedAt}, LastViolationLevelChanged is {account.LastViolationLevelChanged}, LastViolationPointChanged is {account.LastViolationPointChanged}"
        //     });
        // }

        //         // /api/user-service/api/accounts/{AccountId}/deactivate/{IsDeactivate}
        // [HttpPut("{AccountId}/deactivate/{IsDeactivate}")]
        // [Authorize(Policy = "Admin.BasicAccess")]
        // public async Task<IActionResult> DeactivateAccountById(int AccountId, bool IsDeactivate)
        // {
        //     var requestData = JObject.FromObject(new
        //     {
        //         AccountId = AccountId
        //     });
        //     var deactivationFlowName = IsDeactivate ? "user-deactivation-flow" : "user-activation-flow";

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, deactivationFlowName);
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }


        // // /api/user-service/api/accounts/{AccountId}/violation-points/add
        // [HttpPut("{AccountId}/violation-points/add")]
        // [Authorize(Policy = "Admin.BasicAccess")]
        // public async Task<IActionResult> AddViolationPointsToAccountById(ViolationPointChangeRequestDTO violationPointChangeRequestDTO, int AccountId)
        // {
        //     var requestData = JObject.FromObject(new
        //     {
        //         AccountId = AccountId,
        //         ViolationPoint = violationPointChangeRequestDTO.ViolationPoint
        //     });

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "user-violation-punishment-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }


        // // /api/user-service/api/accounts/podcasters/{AccountId}/verify/{IsVerify}
        // [HttpPut("podcasters/{AccountId}/verify/{IsVerify}")]
        // [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        // public async Task<IActionResult> VerifyPodcasterById(int AccountId, bool IsVerify)
        // {
        //     var requestData = JObject.FromObject(new
        //     {
        //         AccountId = AccountId,
        //         IsVerified = IsVerify
        //     });

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "podcaster-verification-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/accounts/{AccountId}/podcast-buddy-reviews
        // [HttpPost("{AccountId}/podcast-buddy-reviews")]
        // [Authorize(Policy = "Customer.NoViolationAccess")]
        // public async Task<IActionResult> GetPodcastBuddyReviewsByAccountId(PodcastBuddyReviewRequestDTO podcastBuddyReviewRequestDTO, int AccountId)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     if (account.Id == AccountId)
        //     {
        //         return StatusCode(403, "You cannot review yourself as a podcast buddy.");
        //     }
        //     if (podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Rating < 0 || podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Rating > 5)
        //     {
        //         return BadRequest("Rating must be between 0 and 5.");
        //     }
        //     var requestData = JObject.FromObject(new
        //     {
        //         AccountId = account.Id,
        //         PodcastBuddyId = AccountId,
        //         Title = podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Title,
        //         Content = podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Content,
        //         Rating = podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Rating
        //     });

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "podcast-buddy-review-creation-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/accounts/podcast-buddy-reviews/{PodcastBuddyReviewId}
        // [HttpPut("podcast-buddy-reviews/{PodcastBuddyReviewId}")]
        // [Authorize(Policy = "Customer.NoViolationAccess")]
        // public async Task<IActionResult> UpdatePodcastBuddyReviewById(PodcastBuddyReviewRequestDTO podcastBuddyReviewRequestDTO, Guid PodcastBuddyReviewId)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     var requestData = JObject.FromObject(new
        //     {
        //         PodcastBuddyReviewId = PodcastBuddyReviewId,
        //         AccountId = account.Id,
        //         Title = podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Title,
        //         Content = podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Content,
        //         Rating = podcastBuddyReviewRequestDTO.PodcastBuddyReviewRequestInfo.Rating
        //     });

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "podcast-buddy-review-update-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/accounts/podcast-buddy-reviews/{PodcastBuddyReviewId}
        // [HttpDelete("podcast-buddy-reviews/{PodcastBuddyReviewId}")]
        // [Authorize(Policy = "Customer.NoViolationAccess")]
        // public async Task<IActionResult> DeletePodcastBuddyReviewById(Guid PodcastBuddyReviewId)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     var requestData = JObject.FromObject(new
        //     {
        //         PodcastBuddyReviewId = PodcastBuddyReviewId,
        //         AccountId = account.Id
        //     });

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "podcast-buddy-review-deletion-flow");
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }

        // // /api/user-service/api/accounts/{AccountId}/{IsFollow}
        // [HttpPost("{AccountId}/follow/{IsFollow}")]
        // [Authorize(Policy = "Customer.BasicAccess")]
        // public async Task<IActionResult> FollowOrUnfollowPodcaster(int AccountId, bool IsFollow)
        // {
        //     var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
        //     if (account.Id == AccountId)
        //     {
        //         return StatusCode(403, "You cannot follow/unfollow yourself.");
        //     }
        //     var requestData = JObject.FromObject(new
        //     {
        //         AccountId = account.Id,
        //         PodcastBuddyId = AccountId,
        //     });
        //     var flowName = IsFollow ? "podcaster-follow-flow" : "podcaster-unfollow-flow";

        //     var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, flowName);
        //     await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        //     return Ok(new
        //     {
        //         SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
        //     }
        //     );
        // }
        #endregion

        // /api/podcast-service/api/channels
        [HttpGet("")]
        public async Task<IActionResult> GetChannels([FromQuery] int? PodcasterId, [FromQuery] string? SearchKeyword, [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var channels = await _podcastChannelService.GetChannels(account?.RoleId);

            return Ok(new
            {
                ChannelList = channels
            });
        }

        // /api/podcast-service/api/channels
        [HttpPost("")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreateChannel(ChannelCreateRequestDTO channelCreateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var channelCreateInfo = JsonConvert.DeserializeObject<ChannelCreateInfoDTO>(channelCreateRequestDTO.ChannelCreateInfo);

            string mainImageFileKey = null;
            if (channelCreateRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.mainImageFileKey", channelCreateRequestDTO.MainImageFile.FileName, channelCreateRequestDTO.MainImageFile.Length, channelCreateRequestDTO.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }


                string newMainImageFileName = $"{Guid.NewGuid()}_{channelCreateRequestDTO.MainImageFile.FileName}";
                using (var stream = channelCreateRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }

                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newMainImageFileName);

            }

            string backgroundImageFileKey = null;
            if (channelCreateRequestDTO.BackgroundImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.backgroundImageFileKey", channelCreateRequestDTO.BackgroundImageFile.FileName, channelCreateRequestDTO.BackgroundImageFile.Length, channelCreateRequestDTO.BackgroundImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }
                string newBackgroundImageFileName = $"{Guid.NewGuid()}_{channelCreateRequestDTO.BackgroundImageFile.FileName}";
                using (var stream = channelCreateRequestDTO.BackgroundImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
                                        newBackgroundImageFileName
                                    );
                }
                backgroundImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newBackgroundImageFileName);
            }

            JObject requestData = JObject.FromObject(channelCreateInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["BackgroundImageFileKey"] = backgroundImageFileKey;
            requestData["PodcasterId"] = account.Id; // lấy PodcasterId từ account đăng nhập hiện tại chứ không phải từ DTO


            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "channel-creation-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/channels/me
        [HttpGet("me")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetMyChannel()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var channels = await _podcastChannelService.GetChannelByPodcasterIdAsync(account.Id);

            return Ok(new
            {
                ChannelList = channels
            });
        }

        // /api/podcast-service/api/channels/{PodcastChannelId}
        [HttpGet("{PodcastChannelId}")]
        public async Task<IActionResult> GetChannelById(Guid PodcastChannelId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var channel = await _podcastChannelService.GetChannelByIdAsync(PodcastChannelId, account?.RoleId);

            return Ok(new
            {
                Channel = channel
            });
        }

        // /api/podcast-service/api/channels/{PodcastChannelId}
        [HttpPut("{PodcastChannelId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> UpdateChannelById(Guid PodcastChannelId, ChannelUpdateRequestDTO channelUpdateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var channelUpdateInfo = JsonConvert.DeserializeObject<ChannelUpdateInfoDTO>(channelUpdateRequestDTO.ChannelUpdateInfo);

            string mainImageFileKey = null;
            if (channelUpdateRequestDTO.MainImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.mainImageFileKey", channelUpdateRequestDTO.MainImageFile.FileName, channelUpdateRequestDTO.MainImageFile.Length, channelUpdateRequestDTO.MainImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }
                string newMainImageFileName = $"{Guid.NewGuid()}_{channelUpdateRequestDTO.MainImageFile.FileName}";
                using (var stream = channelUpdateRequestDTO.MainImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
                                        newMainImageFileName
                                    );
                }
                mainImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newMainImageFileName);
            }
            string backgroundImageFileKey = null;
            if (channelUpdateRequestDTO.BackgroundImageFile != null)
            {
                // bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
                var isValidFile = _fileValidationConfig.IsValidFile("PodcastChannel.backgroundImageFileKey", channelUpdateRequestDTO.BackgroundImageFile.FileName, channelUpdateRequestDTO.BackgroundImageFile.Length, channelUpdateRequestDTO.BackgroundImageFile.ContentType);
                if (!isValidFile)
                {
                    return BadRequest("Invalid upload file.");
                }
                string newBackgroundImageFileName = $"{Guid.NewGuid()}_{channelUpdateRequestDTO.BackgroundImageFile.FileName}";
                using (var stream = channelUpdateRequestDTO.BackgroundImageFile.OpenReadStream())
                {
                    await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                        stream,
                                        _filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH,
                                        newBackgroundImageFileName
                                    );
                }
                backgroundImageFileKey = FilePathHelper.CombinePaths(_filePathConfig.PODCAST_CHANNEL_TEMP_FILE_PATH, newBackgroundImageFileName);
            }
            JObject requestData = JObject.FromObject(channelUpdateInfo);
            requestData["MainImageFileKey"] = mainImageFileKey;
            requestData["BackgroundImageFileKey"] = backgroundImageFileKey;
            requestData["PodcastChannelId"] = PodcastChannelId;
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "channel-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }
    }
}
