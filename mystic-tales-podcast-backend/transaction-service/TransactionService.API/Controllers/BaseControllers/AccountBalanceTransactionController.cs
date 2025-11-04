using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Net.payOS.Types;
using Newtonsoft.Json.Linq;
using TransactionService.API.Filters.ExceptionFilters;
using TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction;
using TransactionService.BusinessLogic.DTOs.Cache;
using TransactionService.BusinessLogic.Enums.App;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Enums.Transaction;
using TransactionService.BusinessLogic.Helpers.FileHelpers;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Common.AppConfigurations.BusinessSetting.interfaces;
using TransactionService.Common.AppConfigurations.FilePath.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.API.Controllers.BaseControllers
{
    [Route("api/account-balance-transactions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class AccountBalanceTransactionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly ILogger<AccountBalanceTransactionController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private const string SAGA_TOPIC = KafkaTopicEnum.PaymentProcessingDomain;
        public AccountBalanceTransactionController(
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            ILogger<AccountBalanceTransactionController> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
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
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreateBalanceDepositPaymentLink([FromBody] AccountBalanceTransactionCreateRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "Amount", request.AccountBalanceTransactionCreateInfo.Amount },
                { "Description", request.AccountBalanceTransactionCreateInfo.Description },
                { "ReturnUrl", request.AccountBalanceTransactionCreateInfo.ReturnUrl ?? string.Empty },
                { "CancelUrl", request.AccountBalanceTransactionCreateInfo.CancelUrl ?? string.Empty }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC, 
                requestData: requestData, 
                sagaInstanceId: null, 
                messageName: "account-balance-create-payment-link-flow");
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

        // /api/transaction-service/api/account-balance-transactions/transfer-receipt-image/get-file-url/{**FileKey}
        [HttpGet("transfer-receipt-image/get-file-url/{**FileKey}")]
        public async Task<IActionResult> GetTransferReceiptImageFileUrl(string FileKey)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            // Validate file key phải là HLS segment
            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.WithdrawalRequestTransferReceiptImage)
            {
                return StatusCode(403, new
                {
                    error = "Invalid file key: Must be a Withdrawal Request Transfer Receipt Image file",
                    actualCategory = category.ToString()
                });
            }
            var url = await _fileIOHelper.GeneratePresignedUrlAsync(FileKey);

            return Ok(new { FileUrl = url });
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
                { "WebHookBody", JObject.FromObject(webhookBody) },
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC, 
                requestData: requestData, 
                sagaInstanceId: null, 
                messageName: "account-balance-confirm-payment-flow");
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
        [HttpPost("balance-withdrawal")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> CreateBalanceWithdrawalRequest([FromBody] AccountBalanceWithdrawalRequestDTO request)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var requestData = new JObject
            {
                { "AccountId", accountId },
                { "Amount", request.Amount },
                { "TransactionTypeId", (int)TransactionTypeEnum.AccountBalanceWithdrawal }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC, 
                requestData: requestData, 
                sagaInstanceId: null, 
                messageName: "account-balance-withdrawal-flow");
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
        [HttpPut("balance-withdrawal/{AccountBalanceWithdrawalRequestId}/confirm/{IsReject}")]
        [Authorize(Policy = "Admin.BasicAccess")]
        public async Task<IActionResult> ConfirmBalanceWithdrawalRequest(
            [FromRoute] bool IsReject,
            [FromRoute] Guid AccountBalanceWithdrawalRequestId,
            [FromBody] AccountBalanceWithdrawalConfirmationRequestDTO request)
        {
            var isValidFile = _fileValidationConfig.IsValidFile("AccountBalanceWithdrawalRequest.transferReceiptImageFileKey", request.TransferReceiptImageFile.FileName, request.TransferReceiptImageFile.Length, request.TransferReceiptImageFile.ContentType);
            if (!isValidFile)
            {
                return BadRequest("Invalid upload file.");
            }

            string newImageFileName = $"{Guid.NewGuid()}_{request.TransferReceiptImageFile.FileName}";
            using (var stream = request.TransferReceiptImageFile.OpenReadStream())
            {
                await _fileIOHelper.UploadBinaryFileWithStreamAsync(
                                    stream,
                                    _filePathConfig.ACCOUNT_TEMP_FILE_PATH,
                                    newImageFileName
                                );
            }
            var imageFileKey = FilePathHelper.CombinePaths(_filePathConfig.ACCOUNT_BALANCE_WITHDRAWAL_REQUEST_TEMP_FILE_PATH, newImageFileName);

            var requestData = new JObject
            {
                { "AccountBalanceWithdrawalRequestId", AccountBalanceWithdrawalRequestId },
                { "ImageFileKey", imageFileKey },
                { "RejectedReason", request.AccountBalanceWithdrawalRequestInfo.RejectedReason ?? string.Empty },
                { "IsReject", IsReject }
            };
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                topic: SAGA_TOPIC, 
                requestData: requestData, 
                sagaInstanceId: null, 
                messageName: "account-balance-withdrawal-confirmation-flow");
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate withdrawal confirmation process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            });
        }
    }
}
