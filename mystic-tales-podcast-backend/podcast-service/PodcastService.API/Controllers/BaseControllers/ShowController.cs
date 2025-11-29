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

        // /api/podcast-service/api/shows
        [HttpGet("")]
        public async Task<IActionResult> GetShows([FromQuery] int? podcaster_id, [FromQuery] string? SearchKeyword, [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var shows = await _podcastShowService.GetShows(account?.RoleId, podcaster_id);

            return Ok(new
            {
                ShowList = shows
            });
        }

        // /api/podcast-service/api/shows/followed
        [HttpGet("followed")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetFollowedShows()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var followedShows = await _podcastShowService.GetFollowedShowsByAccountIdAsync(account.Id);

            return Ok(new
            {
                ShowList = followedShows
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

        // /api/podcast-service/api/shows/dmca-assignable
        [HttpGet("dmca-assignable")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> GetDmcaAssignableShows([FromQuery] string? SearchKeyword, [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 10)
        {
            var shows = await _podcastShowService.GetDmcaAssignableShowsAsync();

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

            var show = await _podcastShowService.GetShowByIdAsync(PodcastShowId, account);

            return Ok(new
            {
                Show = show
            });
        }

        // /api/podcast-service/api/shows/me/{PodcastShowId}
        [HttpGet("me/{PodcastShowId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetMyShowById(Guid PodcastShowId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var show = await _podcastShowService.GetShowByIdForPodcasterAsync(PodcastShowId);

            return Ok(new
            {
                Show = show
            });
        }

        // /api/podcast-service/api/shows/{PodcastShowId}/show-assignable-channels
        [HttpGet("{PodcastShowId}/show-assignable-channels")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> GetShowAssignableChannelsById(Guid PodcastShowId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var assignableChannels = await _podcastShowService.GetShowAssignableChannelsByIdAsync(PodcastShowId, account.Id);

            return Ok(new
            {
                ShowAssignableChannelList = assignableChannels
            });
        }

        // /api/podcast-service/api/shows/{PodcastShowId}/assign-channel
        [HttpPut("{PodcastShowId}/assign-channel")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> AssignChannelToShowById(Guid PodcastShowId, ShowChannelAssignRequestDTO showAssignChannelRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["PodcastShowId"] = PodcastShowId,
                ["PodcasterId"] = account.Id,
                ["PodcastChannelId"] = showAssignChannelRequestDTO.PodcastChannelId
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "show-channel-assignment-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
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

        // /api/podcast-service/api/shows/{PodcastShowId}
        [HttpDelete("{PodcastShowId}")]
        [Authorize(Policy = "Customer.PodcasterAccess")]
        public async Task<IActionResult> DeleteShowById(Guid PodcastShowId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["PodcastShowId"] = PodcastShowId,
                // ["PodcasterId"] = account.Id
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "show-deletion-flow");
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
            if (IsPublish == true && account.ViolationLevel > 0)
            {
                return StatusCode(403, "Your account has violation level that is not allowed to show channel.");
            }

            var flowName = IsPublish == true ? "show-publish-flow" : "show-unpublish-flow";
            JObject requestData = new JObject
            {
                ["PodcastShowId"] = PodcastShowId,
                ["PodcasterId"] = account.Id,
                ["ReleaseDate"] = showPublishRequestDTO.ShowPublishInfo.ReleaseDate?.ToString("yyyy-MM-dd")
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, flowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/shows/{PodcastShowId}/follow/{IsFollow}
        [HttpPost("{PodcastShowId}/follow/{IsFollow}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> FollowOrUnfollowShowById(Guid PodcastShowId, bool IsFollow)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["AccountId"] = account.Id,
                ["PodcastShowId"] = PodcastShowId
            };

            var flowName = IsFollow ? "show-follow-flow" : "show-unfollow-flow";

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, flowName);
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/shows/{PodcastShowId}/podcast-show-reviews
        [HttpPost("{PodcastShowId}/podcast-show-reviews")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreatePodcastShowReviewById(Guid PodcastShowId, PodcastShowReviewCreateRequestDTO podcastShowReviewCreateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            if (podcastShowReviewCreateRequestDTO.PodcastShowReviewCreateInfo.Rating < 0 || podcastShowReviewCreateRequestDTO.PodcastShowReviewCreateInfo.Rating > 5)
            {
                return BadRequest("Rating must be between 0 and 5.");
            }

            JObject requestData = JObject.FromObject(new
            {
                AccountId = account.Id,
                PodcastShowId = PodcastShowId,
                Title = podcastShowReviewCreateRequestDTO.PodcastShowReviewCreateInfo.Title,
                Content = podcastShowReviewCreateRequestDTO.PodcastShowReviewCreateInfo.Content,
                Rating = podcastShowReviewCreateRequestDTO.PodcastShowReviewCreateInfo.Rating
            });


            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "show-review-creation-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/shows/podcast-show-reviews/{PodcastShowReviewId}
        [HttpPut("podcast-show-reviews/{PodcastShowReviewId}")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> UpdatePodcastShowReviewById(Guid PodcastShowReviewId, PodcastShowReviewUpdateRequestDTO podcastShowReviewUpdateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            if (podcastShowReviewUpdateRequestDTO.PodcastShowReviewUpdateInfo.Rating < 0 || podcastShowReviewUpdateRequestDTO.PodcastShowReviewUpdateInfo.Rating > 5)
            {
                return BadRequest("Rating must be between 0 and 5.");
            }

            JObject requestData = JObject.FromObject(new
            {
                AccountId = account.Id,
                PodcastShowReviewId = PodcastShowReviewId,
                Title = podcastShowReviewUpdateRequestDTO.PodcastShowReviewUpdateInfo.Title,
                Content = podcastShowReviewUpdateRequestDTO.PodcastShowReviewUpdateInfo.Content,
                Rating = podcastShowReviewUpdateRequestDTO.PodcastShowReviewUpdateInfo.Rating
            });
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "show-review-update-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }

        // /api/podcast-service/api/shows/podcast-show-reviews/{PodcastShowReviewId}
        [HttpDelete("podcast-show-reviews/{PodcastShowReviewId}")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> DeletePodcastShowReviewById(Guid PodcastShowReviewId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            JObject requestData = new JObject
            {
                ["AccountId"] = account.Id,
                ["PodcastShowReviewId"] = PodcastShowReviewId
            };

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("public-review-management-domain", requestData, null, "show-review-deletion-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }
    }
}
