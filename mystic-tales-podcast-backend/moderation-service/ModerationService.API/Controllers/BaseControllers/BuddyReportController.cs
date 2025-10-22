using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;

namespace ModerationService.API.Controllers.BaseControllers
{
    [Route("api/buddy-reports")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class BuddyReportController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly PodcastBuddyReportService _podcastBuddyReportService;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;

        public BuddyReportController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient,
            PodcastBuddyReportService podcastBuddyReportService,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _podcastBuddyReportService = podcastBuddyReportService;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }

        [HttpGet("test-get-account-status")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> TestGetAccountStatus()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            return Ok(new
            {
                Account = account,
                Message = $"Hello, your account ID is {account.Id}, RoleId is {account.RoleId}, ViolationLevel is {account.ViolationLevel}, ViolationPoint is {account.ViolationPoint}, IsVerified is {account.IsVerified}, DeactivatedAt is {account.DeactivatedAt}, LastViolationLevelChanged is {account.LastViolationLevelChanged}, LastViolationPointChanged is {account.LastViolationPointChanged}"
            });
        }
        [HttpGet]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetBuddyReports()
        {
            var buddyReports = await _podcastBuddyReportService.GetAllPodcastBuddyReportAsync();
            return Ok(new
            {
                BuddyReportList = buddyReports
            });
        }
        [HttpPost("{AccountId}")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreateBuddyReport(
            [FromRoute] int AccountId, 
            [FromBody] PodcastBuddyReportCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var loginAccountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", loginAccountId },
                { "PodcastBuddyId", AccountId },
                { "PodcastBuddyReportTypeId", request.BuddyReportCreateInfo.PodcastBuddyReportTypeId },
                { "Content", request.BuddyReportCreateInfo.Content }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: "report-management-domain",
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-buddy-report-submission-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate buddy report creation.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpGet("buddy-report-types")]
        public async Task<IActionResult> GetBuddyReportType()
        {
            var buddyReportTypeList = await _podcastBuddyReportService.GetPodcastBuddyReportTypeAsync();
            return Ok(new
            {
                BuddyReportTypeList = buddyReportTypeList
            });
        }
        [HttpGet("buddy-report-reviews-sessions")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetBuddyReportReviewSession()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;
            var roleId = account.RoleId;

            var buddyReportReviewSessionList = await _podcastBuddyReportService.GetBuddyReportReviewSessionAsync(accountId, roleId);
            return Ok(new
            {
                BuddyReportReviewSessionList = buddyReportReviewSessionList
            });
        }
        [HttpGet("buddy-report-reviews-sessions/{PodcastBuddyReportReviewSessionId}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> GetBuddyReportReviewSessionById(
            [FromRoute] Guid PodcastBuddyReportReviewSessionId)
        {
            var buddyReportReviewSession = await _podcastBuddyReportService.GetBuddyReportReviewSessionByIdAsync(PodcastBuddyReportReviewSessionId);
            return Ok(new
            {
                BuddyReportReviewSession = buddyReportReviewSession
            });
        }
        [HttpPost("buddy-report-review-sessions/{PodcastBuddyReportReviewSessionId}/resolve/{IsResolved}")]
        [Authorize(Policy = "AdminOrStaff.BasicAccess")]
        public async Task<IActionResult> ResolveBuddyReportReviewSessions(
            [FromRoute] Guid PodcastBuddyReportReviewSessionId,
            [FromRoute] bool IsResolved,
            [FromBody] PodcastBuddyReportReviewSessionResolveRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "IsResolved", IsResolved },
                { "IsTakenEffect", true },
                { "ResolvedViolationPoint", request.ResolvedViolationPoint },
                { "PodcastBuddyReportReviewSessionId", PodcastBuddyReportReviewSessionId }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: "report-management-domain",
                requestData: requestData,
                sagaInstanceId: null,
                messageName: "podcast-buddy-report-resolve-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate resolve buddy report review session.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
