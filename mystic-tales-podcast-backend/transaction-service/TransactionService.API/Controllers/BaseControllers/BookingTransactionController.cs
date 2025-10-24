using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TransactionService.API.Filters.ExceptionFilters;
using TransactionService.BusinessLogic.DTOs.BookingTransaction;
using TransactionService.BusinessLogic.DTOs.Cache;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Enums.Transaction;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.API.Controllers.BaseControllers
{
    [Route("api/booking-transactions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class BookingTransactionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly ILogger<BookingTransactionController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.PaymentProcessingDomain;

        public BookingTransactionController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient,
            ILogger<BookingTransactionController> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }
        [HttpPost("{BookingId}/deposit")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreateBookingDepositTransaction(
            [FromRoute] int BookingId,
            [FromBody] BookingTransactionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "BookingId", BookingId },
                { "AccountId", accountId },
                { "Amount", request.Amount },
                { "TransactionTypeId", (int)TransactionTypeEnum.BookingDeposit }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC, 
                requestData: requestData, 
                sagaInstanceId: null, 
                messageName: "booking-deposit-payment-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate withdrawal process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
        [HttpPost("{BookingId}/pay-the-rest")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreateBookingPayTheRestTransaction(
            [FromRoute] int BookingId,
            [FromBody] BookingTransactionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "BookingId", BookingId },
                { "AccountId", accountId },
                { "Amount", request.Amount },
                { "TransactionTypeId", (int)TransactionTypeEnum.BookingPayTheRest }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC, 
                requestData: requestData, 
                sagaInstanceId: null, 
                messageName: "booking-final-payment-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate pay the rest payment process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
