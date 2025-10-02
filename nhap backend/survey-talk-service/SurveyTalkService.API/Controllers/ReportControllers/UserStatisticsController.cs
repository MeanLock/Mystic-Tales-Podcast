using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.Enums;
using SurveyTalkService.BusinessLogic.Services.DbServices.ReportServices;

namespace SurveyTalkService.API.Controllers.ReportControllers
{
    [Route("api/Report/user")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class UserStatisticsController : ControllerBase
    {
        private ILogger<UserStatisticsController> _logger;
        private readonly UserStatisticsService _userStatisticsService;

        public UserStatisticsController(
            ILogger<UserStatisticsController> logger,
            UserStatisticsService userStatisticsService
            )
        {
            _logger = logger;
            _userStatisticsService = userStatisticsService;
        }

        // GET /api/Report/user/account-registration/summary-count
        [HttpGet("account-registration/summary-count")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> GetAccountRegistrationSummaryReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        {
            if (!report_period.HasValue)
            {
                return BadRequest("ReportPeriod is required.");
            }
            var accountRegistrationSummaryReport = await _userStatisticsService.GetAccountRegistrationSummaryReport(report_period.Value);

            return Ok(accountRegistrationSummaryReport);
        }

        // GET /api/Report/user/platform-feedback
        [HttpGet("platform-feedback")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> GetPlatformFeedbackReport()
        {
            var platformFeedbackReport = await _userStatisticsService.GetPlatformFeedbackReport();

            return Ok(platformFeedbackReport);
        }




        // // GET /api/Report/transaction/community/summary-profit
        // [HttpGet("community/summary-profit")]
        // [Authorize(Policy = "AdminRequiredOnly")]
        // public async Task<IActionResult> GetProfitSummaryReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        // {
        //     if (!report_period.HasValue)
        //     {
        //         return BadRequest("ReportPeriod is required.");
        //     }
        //     var profitSummaryReport = await _transactionStatisticsService.GetCommunityProfitSummaryReport(report_period.Value);

        //     return Ok(profitSummaryReport);
        // }

        // // GET /api/Report/transaction/community/periodic-profit
        // [HttpGet("community/periodic-profit")]
        // [Authorize(Policy = "AdminRequiredOnly")]
        // public async Task<IActionResult> GetProfitPeriodicReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        // {
        //     if (!report_period.HasValue)
        //     {
        //         return BadRequest("ReportPeriod is required.");
        //     }
        //     var profitPeriodicReport = await _transactionStatisticsService.GetCommunityProfitPeriodicReport(report_period.Value);

        //     return Ok(profitPeriodicReport);
        // }

        // // GET /api/Report/transaction/account-balance/periodic-count  
        // [HttpGet("account-balance/periodic-count")]
        // [Authorize(Policy = "AdminRequiredOnly")]
        // public async Task<IActionResult> GetAccountBalancePeriodicTransactionCountReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        // {
        //     if (!report_period.HasValue)
        //     {
        //         return BadRequest("ReportPeriod is required.");
        //     }
        //     var accountBalancePeriodicTransactionCountReport = await _transactionStatisticsService.GetAccountBalancePeriodicTransactionCountReport(report_period.Value);

        //     return Ok(accountBalancePeriodicTransactionCountReport);
        // }

        // // GET /api/Report/transaction/account-balance/summary-count
        // [HttpGet("account-balance/summary-count")]
        // [Authorize(Policy = "AdminRequiredOnly")]
        // public async Task<IActionResult> GetAccountBalanceSummaryTransactionCountReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        // {
        //     if (!report_period.HasValue)
        //     {
        //         return BadRequest("ReportPeriod is required.");
        //     }
        //     var accountBalanceSummaryTransactionCountReport = await _transactionStatisticsService.GetAccountBalanceSummaryTransactionCountReport(report_period.Value);

        //     return Ok(accountBalanceSummaryTransactionCountReport);
        // }
    }
}
