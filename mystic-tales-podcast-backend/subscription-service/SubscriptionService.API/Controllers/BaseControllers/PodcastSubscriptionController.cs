using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.DTOs.Cache;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.Infrastructure.Services.Kafka;

namespace SubscriptionService.API.Controllers.BaseControllers
{
    [Route("api/podcast-subscriptions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class PodcastSubscriptionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly PodcastSubscriptionService _podcastSubscriptionService;
        private readonly ILogger<PodcastSubscriptionController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;

        public PodcastSubscriptionController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            PodcastSubscriptionService podcastSubscriptionService,
            ILogger<PodcastSubscriptionController> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _podcastSubscriptionService = podcastSubscriptionService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }
        [HttpGet("shows/{PodcastShowId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionByPodcastShowId([FromRoute] Guid PodcastShowId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _podcastSubscriptionService.GetPodcastShow(accountId, PodcastShowId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Show Id: {PodcastShowId}");
            }
            var podcastSubscription = await _podcastSubscriptionService.GetPodcastSubscriptionListByPodcastShowIdAsync(PodcastShowId);
            if (podcastSubscription == null)
            {
                return NotFound($"No podcast subscription found with Show Id: {PodcastShowId}");
            }
            return Ok(podcastSubscription);
        }
        [HttpPost("shows/{PodcastShowId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreatePodcastSubscription(
            [FromRoute] Guid PodcastShowId,
            [FromBody] PodcastSubscriptionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isValid = await _podcastSubscriptionService.GetPodcastShow(accountId, PodcastShowId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastShow with Id: {PodcastShowId}");
            }

            var requestData = new JObject
            {
                { "Name", request.Name },
                { "Description", request.Description },
                { "PodcastShowId", PodcastShowId  },
                { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(request.PodcastSubscriptionCycleTypePriceCreateInfoList) },
                { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(request.PodcastSubscriptionBenefitMappingCreateInfoList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("subscription-management-domain", requestData, null, "podcast-subscription-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("channels/{PodcastChannelId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionByPodcastChannelId([FromRoute] Guid PodcastChannelId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _podcastSubscriptionService.GetPodcastChannel(accountId, PodcastChannelId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Channel Id: {PodcastChannelId}");
            }
            var podcastSubscription = await _podcastSubscriptionService.GetPodcastSubscriptionListByPodcastChannelIdAsync(PodcastChannelId);
            if (podcastSubscription == null)
            {
                return NotFound($"No podcast subscription found with Channel Id: {PodcastChannelId}");
            }
            return Ok(podcastSubscription);
        }
        [HttpPost("channels/{PodcastChannelId}")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> CreatePodcastSubscriptionByChannelId(
            [FromRoute] Guid PodcastChannelId,
            [FromBody] PodcastSubscriptionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isValid = await _podcastSubscriptionService.GetPodcastChannel(accountId, PodcastChannelId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastChannel with Id: {PodcastChannelId}");
            }
            var requestData = new JObject
            {
                { "Name", request.Name },
                { "Description", request.Description },
                { "PodcastChannelId", PodcastChannelId  },
                { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(request.PodcastSubscriptionCycleTypePriceCreateInfoList) },
                { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(request.PodcastSubscriptionBenefitMappingCreateInfoList) }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("subscription-management-domain", requestData, null, "podcast-subscription-creation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate podcast subscription creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
