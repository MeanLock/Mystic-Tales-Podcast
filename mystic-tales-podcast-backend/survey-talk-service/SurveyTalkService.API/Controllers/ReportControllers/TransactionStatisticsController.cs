using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.Enums;
using SurveyTalkService.BusinessLogic.Services.DbServices.ReportServices;

namespace SurveyTalkService.API.Controllers.ReportControllers
{
    [Route("api/Report/transaction")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class TransactionStatisticsController : ControllerBase
    {
        private ILogger<TransactionStatisticsController> _logger;
        private readonly TransactionStatisticsService _transactionStatisticsService;

        public TransactionStatisticsController(
            ILogger<TransactionStatisticsController> logger,
            TransactionStatisticsService transactionStatisticsService
            )
        {
            _logger = logger;
            _transactionStatisticsService = transactionStatisticsService;
        }

        // GET /api/Report/transaction/community/summary-profit
        [HttpGet("community/summary-profit")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> GetProfitSummaryReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        {
            if (!report_period.HasValue)
            {
                return BadRequest("ReportPeriod is required.");
            }
            var profitSummaryReport = await _transactionStatisticsService.GetCommunityProfitSummaryReport(report_period.Value);

            return Ok(profitSummaryReport);
        }

        // GET /api/Report/transaction/community/periodic-profit
        [HttpGet("community/periodic-profit")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> GetProfitPeriodicReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        {
            if (!report_period.HasValue)
            {
                return BadRequest("ReportPeriod is required.");
            }
            var profitPeriodicReport = await _transactionStatisticsService.GetCommunityProfitPeriodicReport(report_period.Value);

            return Ok(profitPeriodicReport);
        }

        // GET /api/Report/transaction/account-balance/periodic-amount  
        [HttpGet("account-balance/periodic-amount")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> GetAccountBalancePeriodicTransactionAmountReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        {
            if (!report_period.HasValue)
            {
                return BadRequest("ReportPeriod is required.");
            }
            var accountBalancePeriodicTransactionCountReport = await _transactionStatisticsService.GetAccountBalancePeriodicTransactionAmountReport(report_period.Value);

            return Ok(accountBalancePeriodicTransactionCountReport);
        }

        // GET /api/Report/transaction/account-balance/summary-count
        [HttpGet("account-balance/summary-count")]
        [Authorize(Policy = "AdminRequiredOnly")]
        public async Task<IActionResult> GetAccountBalanceSummaryTransactionCountReport([FromQuery] StatisticsReportPeriodEnum? report_period = null)
        {
            if (!report_period.HasValue)
            {
                return BadRequest("ReportPeriod is required.");
            }
            var accountBalanceSummaryTransactionCountReport = await _transactionStatisticsService.GetAccountBalanceSummaryTransactionCountReport(report_period.Value);

            return Ok(accountBalanceSummaryTransactionCountReport);
        }
        


        
    }
}
