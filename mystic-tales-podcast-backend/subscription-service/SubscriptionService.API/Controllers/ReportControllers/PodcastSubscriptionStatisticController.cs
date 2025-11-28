using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.API.Controllers.BaseControllers;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.DTOs.Cache;
using SubscriptionService.BusinessLogic.Enums.Kafka;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.Infrastructure.Services.Kafka;

namespace SubscriptionService.API.Controllers.ReportControllers
{
    [Route("api/report/podcast-subscriptions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class PodcastSubscriptionStatisticController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly PodcastSubscriptionService _podcastSubscriptionService;
        private readonly ILogger<PodcastSubscriptionStatisticController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.SubscriptionManagementDomain;

        public PodcastSubscriptionStatisticController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            PodcastSubscriptionService podcastSubscriptionService,
            ILogger<PodcastSubscriptionStatisticController> logger,
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
        [HttpGet("channels/{PodcastChannelId}/dashboard")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionDashboardByPodcastChannelId([FromRoute] Guid PodcastChannelId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isValid = await _podcastSubscriptionService.GetPodcastChannelWithAccountId(accountId, PodcastChannelId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Channel Id: {PodcastChannelId}");
            }
            var podcastSubscriptionDashboard = await _podcastSubscriptionService.GetPodcastSubscriptionDashboardByPodcastChannelIdAsync(PodcastChannelId);
            return Ok(podcastSubscriptionDashboard);
        }
        [HttpGet("shows/{PodcastShowId}/dashboard")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionDashboardByPodcastShowlId([FromRoute] Guid PodcastShowId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var isValid = await _podcastSubscriptionService.GetPodcastShowWithAccountId(accountId, PodcastShowId);
            if (isValid == null)
            {
                return Forbid($"The Logged In Account is unauthorized to access Podcast Show Id: {PodcastShowId}");
            }
            var podcastSubscriptionDashboard = await _podcastSubscriptionService.GetPodcastSubscriptionDashboardByPodcastShowIdAsync(PodcastShowId);
            return Ok(podcastSubscriptionDashboard);
        }

    }
}
