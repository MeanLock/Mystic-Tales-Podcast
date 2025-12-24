using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;

namespace ModerationService.API.Controllers.BaseControllers
{
    [Route("api/episode-reports")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class EpisodeReportController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly PodcastEpisodeReportService _podcastEpisodeReportService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.ReportManagementDomain;
        public EpisodeReportController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient,
            PodcastEpisodeReportService podcastEpisodeReportService,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _podcastEpisodeReportService = podcastEpisodeReportService;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }
        [HttpGet]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetEpisodeReports()
        {
            var episodeReports = await _podcastEpisodeReportService.GetAllPodcastEpisodeReportAsync();
            return Ok(new
            {
                EpisodeReportList = episodeReports
            });
        }
        [HttpPost("{PodcastEpisodeId}")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreateEpisodeReport(
            [FromRoute] Guid PodcastEpisodeId,
            [FromBody] PodcastEpisodeReportCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", loginAccountId },
                { "PodcastEpisodeId", PodcastEpisodeId },
                { "PodcastEpisodeReportTypeId", request.EpisodeReportCreateInfo.PodcastEpisodeReportTypeId },
                { "Content", request.EpisodeReportCreateInfo.Content }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "episode-report-submission-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate episode report creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("episode-report-types")]
        public async Task<IActionResult> GetAllEpisodeReportType()
        {
            var EpisodeReportTypeList = await _podcastEpisodeReportService.GetAllPodcastEpisodeReportTypeAsync();
            return Ok(new
            {
                EpisodeReportTypeList = EpisodeReportTypeList
            });
        }
        [HttpGet("episodes/{PodcastEpisodeId}/episode-report-types")]
        public async Task<IActionResult> GetEpisodeReportType([FromRoute] Guid PodcastEpisodeId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var EpisodeReportTypeList = await _podcastEpisodeReportService.GetPodcastEpisodeReportTypeAsync(account, PodcastEpisodeId);
            return Ok(new
            {
                EpisodeReportTypeList = EpisodeReportTypeList
            });
        }
        [HttpGet("episode-report-review-sessions")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetEpisodeReportReviewSession()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var roleId = account.RoleId;

            var EpisodeReportReviewSessionList = await _podcastEpisodeReportService.GetEpisodeReportReviewSessionAsync(accountId, roleId);
            return Ok(new
            {
                EpisodeReportReviewSessionList = EpisodeReportReviewSessionList
            });
        }
        [HttpGet("episode-report-review-sessions/{PodcastEpisodeReportReviewSessionId}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetEpisodeReportReviewSessionById(
            [FromRoute] Guid PodcastEpisodeReportReviewSessionId)
        {
            var EpisodeReportReviewSession = await _podcastEpisodeReportService.GetEpisodeReportReviewSessionByIdAsync(PodcastEpisodeReportReviewSessionId);
            return Ok(new
            {
                EpisodeReportReviewSession = EpisodeReportReviewSession
            });
        }
        [HttpPost("episode-report-review-sessions/{PodcastEpisodeReportReviewSessionId}/resolve/{IsResolved}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> ResolveEpisodeReportReviewSessions(
            [FromRoute] Guid PodcastEpisodeReportReviewSessionId,
            [FromRoute] bool IsResolved)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "IsResolved", IsResolved },
                { "IsTakenEffect", true },
                { "PodcastEpisodeReportReviewSessionId", PodcastEpisodeReportReviewSessionId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-episode-report-resolve-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate resolve episode report review session.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }

    }
}
