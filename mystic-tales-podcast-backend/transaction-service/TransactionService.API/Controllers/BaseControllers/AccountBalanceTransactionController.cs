using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Net.payOS.Types;
using Newtonsoft.Json.Linq;
using TransactionService.API.Filters.ExceptionFilters;
using TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction;
using TransactionService.BusinessLogic.DTOs.Cache;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.API.Controllers.BaseControllers
{
    [Route("api/account-balance-transactions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountBalanceTransactionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly ILogger<AccountBalanceTransactionController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;

        public AccountBalanceTransactionController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient,
            ILogger<AccountBalanceTransactionController> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _logger = logger;
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
        [HttpPost("balance-deposit/create-payment-link")]
        public async Task<IActionResult> CreateBalanceDepositPaymentLink([FromBody] AccountBalanceTransactionRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject 
            { 
                { "AccountId", accountId },
                { "Amount", request.Amount },
                { "Description", request.Description },
                { "ReturnUrl", request.ReturnUrl ?? string.Empty },
                { "CancelUrl", request.CancelUrl ?? string.Empty }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("payment-processing-domain", requestData, null, "account-balance-create-payment-link-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate create payment link process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] WebhookType? webhookBody)
        {
            if (webhookBody == null)
            {
                return BadRequest("Invalid webhook data.");
            }
            // Process the webhook data as needed
            _logger.LogInformation("Received payment confirmation webhook: {WebhookBody}", JObject.FromObject(webhookBody).ToString());

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "Amount", request.Amount },
                { "Description", request.Description },
                { "ReturnUrl", request.ReturnUrl ?? string.Empty },
                { "CancelUrl", request.CancelUrl ?? string.Empty }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("payment-processing-domain", requestData, null, "account-balance-create-payment-link-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate create payment link process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
