using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionService.API.Controllers.BaseControllers;
using TransactionService.API.Filters.ExceptionFilters;
using TransactionService.BusinessLogic.DTOs.Cache;
using TransactionService.BusinessLogic.Enums;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Helpers.FileHelpers;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.DbServices.TransactionServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Common.AppConfigurations.BusinessSetting.interfaces;
using TransactionService.Common.AppConfigurations.FilePath.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.API.Controllers.ReportControllers
{
    [Route("api/report/account-balance-transactions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class AccountBalanceStatisticController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AccountBalanceTransactionService _accountBalanceTransactionService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly ILogger<AccountBalanceStatisticController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.PaymentProcessingDomain;
        public AccountBalanceStatisticController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            AccountBalanceTransactionService accountBalanceTransactionService,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            ILogger<AccountBalanceStatisticController> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _accountBalanceTransactionService = accountBalanceTransactionService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }
        [HttpGet("statistics/summary/me")]
        [Authorize(Policy = "Customer.NoViolationAccess.PodcasterAccess")]
        public async Task<IActionResult> GetAccountBalanceTransactionSummaryStatisticForCurrentAccountAsync(
            [FromQuery] StatisticsReportPeriodEnum? report_period = null)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            if (!report_period.HasValue)
            {
                return BadRequest("ReportPeriod is required.");
            }
            var accountBalanceTransactionStatistic = await _accountBalanceTransactionService.GetAccountBalanceTransactionStatisticByPodcasterIdAsync(report_period.Value, account);

            return Ok(accountBalanceTransactionStatistic);
        }
        [HttpGet("statistics/summary/system")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> GetSystemAccountBalanceTransactionSummaryStatisticAsync(
            [FromQuery] StatisticsReportPeriodEnum? report_period = null)
        {
            if (!report_period.HasValue)
            {
                return BadRequest("ReportPeriod is required.");
            }
            var accountBalanceTransactionStatistic = await _accountBalanceTransactionService.GetSystemAccountBalanceTransactionStatisticAsync(report_period.Value);

            return Ok(accountBalanceTransactionStatistic);
        }
    }
}
