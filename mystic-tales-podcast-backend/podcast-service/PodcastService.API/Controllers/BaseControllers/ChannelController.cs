using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/channels")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class ChannelController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public ChannelController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        #region Sample coding format
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
        // [HttpGet]

        // /api/podcast-service/api/channels
        [HttpPost("/channels")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreateChannel(ChannelCreateRequestDTO channelCreateRequestDTO)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var newChannel = await _genericQueryService.CreateChannelAsync(account.Id, createChannelRequestDTO);
            return Ok(new
            {
                Channel = newChannel
            });
        }



    }
}
