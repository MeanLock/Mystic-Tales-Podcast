using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;

namespace ModerationService.API.Controllers.BaseControllers
{
    [Route("api/show-reports")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class ShowReportController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly PodcastShowReportService _podcastShowReportService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.ReportManagementDomain;
        public ShowReportController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient,
            PodcastShowReportService podcastShowReportService,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _podcastShowReportService = podcastShowReportService;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetShowReports()
        {
            var showReports = await _podcastShowReportService.GetAllPodcastShowReportAsync();
            return Ok(new
            {
                ShowReportList = showReports
            });
        }
        [HttpPost("{PodcastShowId}")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreateShowReport(
            [FromRoute] Guid PodcastShowId,
            [FromBody] PodcastShowReportCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", loginAccountId },
                { "PodcastShowId", PodcastShowId },
                { "PodcastShowReportTypeId", request.ShowReportCreateInfo.PodcastShowReportTypeId },
                { "Content", request.ShowReportCreateInfo.Content }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "show-report-submission-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate show report creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("show-report-types")]
        public async Task<IActionResult> GetShowReportType()
        {
            var showReportTypeList = await _podcastShowReportService.GetPodcastShowReportTypeAsync();
            return Ok(new
            {
                ShowReportTypeList = showReportTypeList
            });
        }
        [HttpGet("show-report-reviews-sessions")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetShowReportReviewSession()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var roleId = account.RoleId;

            var showReportReviewSessionList = await _podcastShowReportService.GetShowReportReviewSessionAsync(accountId, roleId);
            return Ok(new
            {
                ShowReportReviewSessionList = showReportReviewSessionList
            });
        }
        [HttpGet("show-report-reviews-sessions/{PodcastShowReportReviewSessionId}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetShowReportReviewSessionById(
            [FromRoute] Guid PodcastShowReportReviewSessionId)
        {
            var showReportReviewSession = await _podcastShowReportService.GetShowReportReviewSessionByIdAsync(PodcastShowReportReviewSessionId);
            return Ok(new
            {
                ShowReportReviewSession = showReportReviewSession
            });
        }
        [HttpPost("show-report-review-sessions/{PodcastShowReportReviewSessionId}/resolve/{IsResolved}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> ResolveShowReportReviewSessions(
            [FromRoute] Guid PodcastShowReportReviewSessionId,
            [FromRoute] bool IsResolved)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "IsResolved", IsResolved },
                { "IsTakenEffect", true }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC,
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-show-report-resolve-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate resolve show report review session.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
