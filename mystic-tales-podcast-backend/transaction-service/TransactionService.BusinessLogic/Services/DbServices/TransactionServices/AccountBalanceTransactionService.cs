using GreenDonut;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Net.payOS;
using Net.payOS.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction;
using TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction.ListItems;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.AccountBalanceCreatePaymentLink;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmAccountBalanceWithdrawal;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPayment;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPaymentRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateAccountBalanceTransactionRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateWithdrawalRequest;
using TransactionService.BusinessLogic.DTOs.Transaction;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Enums.Transaction;
using TransactionService.BusinessLogic.Helpers.DateHelpers;
using TransactionService.BusinessLogic.Helpers.FileHelpers;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Common.AppConfigurations.BusinessSetting.interfaces;
using TransactionService.Common.AppConfigurations.FilePath.interfaces;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities.SqlServer;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.DataAccess.UOW;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Infrastructure.Models.Kafka;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.BusinessLogic.Services.DbServices.TransactionServices
{
    public class AccountBalanceTransactionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<AccountBalanceTransactionService> _logger;
        private readonly IGenericRepository<AccountBalanceTransaction> _accountBalanceTransactionGenericRepository;
        private readonly IGenericRepository<AccountBalanceWithdrawalRequest> _accountBalanceWithdrawalRequestGenericRepository;

        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        private readonly IPayosConfig _payosConfig;

        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly DateHelper _dateHelper;
        public AccountBalanceTransactionService(
            AppDbContext appDbContext, 
            ILogger<AccountBalanceTransactionService> logger,
            IGenericRepository<AccountBalanceTransaction> accountBalanceTransactionGenericRepository,
            IGenericRepository<AccountBalanceWithdrawalRequest> accountBalanceWithdrawalRequestGenericRepository,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            IPayosConfig payosConfig,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            DateHelper dateHelper)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _accountBalanceTransactionGenericRepository = accountBalanceTransactionGenericRepository;
            _accountBalanceWithdrawalRequestGenericRepository = accountBalanceWithdrawalRequestGenericRepository;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _payosConfig = payosConfig;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _dateHelper = dateHelper;
        }
        public async Task CreateAccountBalanceTransactionDepositPaymentLinkAsync(AccountBalanceCreatePaymentLinkParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    PayOS payOS = new PayOS(_payosConfig.ClientID, _payosConfig.APIKey, _payosConfig.ChecksumKey);
                    long orderCode = await GenerateRandomLongAsync();

                    AccountBalanceTransaction accountBalanceTransaction = new AccountBalanceTransaction
                    {
                        AccountId = parameter.AccountId,
                        Amount = parameter.Amount,
                        OrderCode = orderCode.ToString(),
                        TransactionTypeId = (int)TransactionTypeEnum.AccountBalanceDeposits,
                        TransactionStatusId = (int)TransactionStatusEnum.Pending,
                    };

                    // Save payment history to the database
                    AccountBalanceTransaction newAccountBalanceTransaction = await _accountBalanceTransactionGenericRepository.CreateAsync(accountBalanceTransaction);
                    

                    PaymentData paymentData = new PaymentData(orderCode, (int)parameter.Amount, parameter.Description ?? "MTP BANKING NAP TIEN",
                                             [], parameter.CancelUrl, parameter.ReturnUrl);

                    CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
                    await transaction.CommitAsync();

                    var newResponseData = new JObject{
                        { "PaymentLinkUrl", createPayment.checkoutUrl },
                        { "AccountBalanceTransactionId", newAccountBalanceTransaction.Id}
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Account balance create payment link successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while create payment link for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Account balance create payment link failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Account balance create payment link failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ConfirmAccountBalanceTransactionPaymentAsync(ConfirmPaymentParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    WebhookType webhookBody = parameter.WebHookBody;
                    if (webhookBody == null)
                    {
                        throw new Exception("Invalid webhook data.");
                    }
                    if (webhookBody.data.description == "VQRIO123")
                    {
                        return;
                    }
                    // Console.WriteLine("\n\n\n" + JsonConvert.SerializeObject(webhookBody, Formatting.Indented) + "\n\n\n");

                    // Retrieve the payment history record
                    AccountBalanceTransaction? accountBalanceTransaction = _accountBalanceTransactionGenericRepository.FindAll()
                        .Where(abt => abt.OrderCode.Equals(webhookBody.data.orderCode.ToString()))
                        .FirstOrDefault();
                    if (accountBalanceTransaction == null)
                    {
                        throw new HttpRequestException("Payment not found.");
                    }

                    if (webhookBody.code == "00")
                    {
                        accountBalanceTransaction.TransactionStatusId = (int)TransactionStatusEnum.Success;
                    }
                    else
                    {
                        accountBalanceTransaction.TransactionStatusId = (int)TransactionStatusEnum.Error;
                    }

                    accountBalanceTransaction.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _accountBalanceTransactionGenericRepository.UpdateAsync(accountBalanceTransaction.Id, accountBalanceTransaction);

                    await transaction.CommitAsync();

                    var newRequestData = new JObject{
                        { "AccountBalanceTransactionId", accountBalanceTransaction.Id },
                        { "AccountId", accountBalanceTransaction.AccountId },
                        { "Amount", accountBalanceTransaction.Amount }
                    };
                    var newResponseData = new JObject{
                        { "AccountBalanceTransactionId", accountBalanceTransaction.Id },
                        { "AccountId", accountBalanceTransaction.AccountId },
                        { "Amount", accountBalanceTransaction.Amount }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Account balance confirm payment successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while confirm payment for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Account balance confirm payment failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Account balance confirm payment failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ConfirmAccountBalanceTransactionPaymentRollbackAsync(ConfirmPaymentRollbackParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    AccountBalanceTransaction? accountBalanceTransaction = _accountBalanceTransactionGenericRepository.FindByIdAsync(parameter.AccountBalanceTransactionId).Result;
                    if (accountBalanceTransaction == null)
                    {
                        throw new Exception("Payment not found.");
                    }
                    ;
                    accountBalanceTransaction.TransactionStatusId = (int)TransactionStatusEnum.Error; // Thay đổi trạng thái giao dịch thành "Thất bại"
                    accountBalanceTransaction.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _accountBalanceTransactionGenericRepository.UpdateAsync(accountBalanceTransaction.Id, accountBalanceTransaction);
                    await transaction.CommitAsync();
                    var newResponseData = new JObject{
                        { "AccountBalanceTransactionId", accountBalanceTransaction.Id },
                        { "TransactionStatusId", accountBalanceTransaction.TransactionStatusId}
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Rollback confirm payment successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while rolling back confirm payment for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Rollback confirm payment failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Rollback confirm payment failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CreateAccountBalanceTransactionRollbackAsync(CreateAccountBalanceTransactionRollbackParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    AccountBalanceTransaction? accountBalanceTransaction = _accountBalanceTransactionGenericRepository.FindByIdAsync(parameter.AccountBalanceTransactionId).Result;
                    if (accountBalanceTransaction == null)
                    {
                        throw new Exception("Account Balance Transaction not found.");
                    }
                    ;
                    accountBalanceTransaction.TransactionStatusId = (int)TransactionStatusEnum.Error; // Thay đổi trạng thái giao dịch thành "Thất bại"
                    accountBalanceTransaction.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _accountBalanceTransactionGenericRepository.UpdateAsync(accountBalanceTransaction.Id, accountBalanceTransaction);
                    await transaction.CommitAsync();
                    var newResponseData = new JObject{
                        { "AccountBalanceTransactionId", accountBalanceTransaction.Id },
                        { "TransactionStatusId", accountBalanceTransaction.TransactionStatusId}
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Rollback create account balance transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while rolling back create account balance transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Rollback create account balance transactionnt failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Rollback create account balance transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CreateAccountBalanceTransactionWithdrawalRequestAsync(CreateWithdrawalRequestParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var newAccountBalanceWithdrawalRequest = new AccountBalanceWithdrawalRequest
                    {
                        AccountId = parameter.AccountId,
                        Amount = parameter.Amount,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    var createdAccountBalanceWithdrawalRequest = await _accountBalanceWithdrawalRequestGenericRepository.CreateAsync(newAccountBalanceWithdrawalRequest);

                    await transaction.CommitAsync();
                    var newResponseData = new JObject{
                        { "AccountBalanceWithdrawalRequestId", createdAccountBalanceWithdrawalRequest.Id },
                        { "CreatedAt", createdAccountBalanceWithdrawalRequest.CreatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Creating account balance withdrawal request successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating account balance withdrawal request for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Creating account balance withdrawal request failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Creating account balance withdrawal request failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ConfirmAccountBalanceWithdrawalAsync(ConfirmAccountBalanceWithdrawalParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var accountBalanceWithdrawalRequest = await _accountBalanceWithdrawalRequestGenericRepository.FindByIdAsync(parameter.AccountBalanceWithdrawalRequestId);
                    if (accountBalanceWithdrawalRequest == null)
                    {
                        throw new Exception("Account balance withdrawal request not found.");
                    }
                    if (parameter.IsReject)
                    {
                        accountBalanceWithdrawalRequest.RejectReason = parameter.RejectedReason;
                        accountBalanceWithdrawalRequest.IsRejected = parameter.IsReject;
                        accountBalanceWithdrawalRequest.CompletedAt = _dateHelper.GetNowByAppTimeZone();
                        accountBalanceWithdrawalRequest.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _accountBalanceWithdrawalRequestGenericRepository.UpdateAsync(accountBalanceWithdrawalRequest.Id, accountBalanceWithdrawalRequest);
                    }
                    else
                    {
                        accountBalanceWithdrawalRequest.IsRejected = parameter.IsReject;
                        accountBalanceWithdrawalRequest.CompletedAt = _dateHelper.GetNowByAppTimeZone();
                        accountBalanceWithdrawalRequest.UpdatedAt = _dateHelper.GetNowByAppTimeZone();

                        var folderPath = _filePathConfig.ACCOUNT_BALANCE_WITHDRAWAL_REQUEST_FILE_PATH + "\\" + accountBalanceWithdrawalRequest.Id;
                        if (parameter.ImageFileKey != null && parameter.ImageFileKey != "")
                        {
                            throw new Exception("Image file key is required.");
                        }
                        var newImageFileKey = FilePathHelper.CombinePaths(folderPath, $"transfer_receipt_image{FilePathHelper.GetExtension(parameter.ImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(parameter.ImageFileKey, newImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(parameter.ImageFileKey);
                        accountBalanceWithdrawalRequest.TransferReceiptImageFileKey = newImageFileKey;
                        await _accountBalanceWithdrawalRequestGenericRepository.UpdateAsync(accountBalanceWithdrawalRequest.Id, accountBalanceWithdrawalRequest);

                        var subtractAccountBalanceRequest = new JObject
                        {
                            { "AccountId", accountBalanceWithdrawalRequest.AccountId },
                            { "Amount", accountBalanceWithdrawalRequest.Amount }
                        };
                        var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.PaymentProcessingDomain,
                            requestData: subtractAccountBalanceRequest,
                            messageName: "account-balance-subtraction-flow",
                            sagaInstanceId: sagaId);
                        await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage, sagaId.ToString());
                        _logger.LogInformation("Started account balance subtraction flow for Account Balance Withdrawal Request Id: {AccountBalanceWithdrawalRequestId}", accountBalanceWithdrawalRequest.Id);
                    }

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    newResponseData["UpdatedAt"] = accountBalanceWithdrawalRequest.UpdatedAt;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Confirm account balance withdrawal request successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while confirming account balance withdrawal request for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Confirm account balance withdrawal request failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Confirm account balance withdrawal request failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<List<AccountBalanceTransactionListItemResponseDTO>?> GetAccountBalanceTransactionsAsync(int accountId, AccountBalanceTypeEnum typeEnum)
        {
            try
            {
                switch (typeEnum)
                {
                    case AccountBalanceTypeEnum.MoneyIn:
                        return await _accountBalanceTransactionGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(a => a.TransactionType)
                            .Include(a => a.TransactionStatus))
                            .Where(abt => abt.AccountId == accountId && abt.TransactionTypeId == (int)TransactionTypeEnum.AccountBalanceDeposits)
                            .OrderByDescending(abt => abt.CreatedAt)
                            .Select(a => new AccountBalanceTransactionListItemResponseDTO
                            {
                                Id = a.Id,
                                Amount = a.Amount,
                                TransactionType = new TransactionTypeResponseDTO
                                {
                                    Id = a.TransactionType.Id,
                                    Name = a.TransactionType.Name
                                },
                                TransactionStatus = new TransactionStatusResponseDTO
                                {
                                    Id = a.TransactionStatus.Id,
                                    Name = a.TransactionStatus.Name
                                },
                                CreatedAt = a.CreatedAt,
                                ChangedAt = a.TransactionStatusId == (int)TransactionStatusEnum.Success || a.TransactionStatusId == (int)TransactionStatusEnum.Error || a.TransactionStatus.Id == (int)TransactionStatusEnum.Cancelled ? a.UpdatedAt : null
                            })
                            .ToListAsync();
                    case AccountBalanceTypeEnum.MoneyOut:
                        return await _accountBalanceTransactionGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(a => a.TransactionType)
                            .Include(a => a.TransactionStatus))
                            .Where(abt => abt.AccountId == accountId && abt.TransactionTypeId == (int)TransactionTypeEnum.AccountBalanceWithdrawal)
                            .OrderByDescending(abt => abt.CreatedAt)
                            .Select(a => new AccountBalanceTransactionListItemResponseDTO
                            {
                                Id = a.Id,
                                Amount = a.Amount,
                                TransactionType = new TransactionTypeResponseDTO
                                {
                                    Id = a.TransactionType.Id,
                                    Name = a.TransactionType.Name
                                },
                                TransactionStatus = new TransactionStatusResponseDTO
                                {
                                    Id = a.TransactionStatus.Id,
                                    Name = a.TransactionStatus.Name
                                },
                                CreatedAt = a.CreatedAt,
                                ChangedAt = a.TransactionStatusId == (int)TransactionStatusEnum.Success || a.TransactionStatusId == (int)TransactionStatusEnum.Error || a.TransactionStatus.Id == (int)TransactionStatusEnum.Cancelled ? a.UpdatedAt : null
                            })
                            .ToListAsync();
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving account balance transactions for AccountId: {AccountId}", accountId);
                throw new HttpRequestException("An error occurred while retrieving account balance transactions.");
            }
        }
        public async Task<PaymentResultResponseDTO> GetAccountBalanceTransactionByOrderCodeAsync(string orderCode)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await _accountBalanceTransactionGenericRepository.FindAll()
                    .Where(abt => abt.OrderCode == orderCode && abt.TransactionStatusId == (int)TransactionStatusEnum.Success)
                    .FirstOrDefaultAsync();

                    result.OrderCode = orderCode + "*";
                    await _accountBalanceTransactionGenericRepository.UpdateAsync(result.Id, result);

                    await transaction.CommitAsync();

                    return new PaymentResultResponseDTO
                    {
                        Amount = result.Amount,
                        CompletedAt = result.UpdatedAt
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while retrieving Payment Result");
                    throw new HttpRequestException("An error occurred while retrieving Payment Result.");
                }
            }

        }
        public async Task<long> GenerateRandomLongAsync(int minDigits = 5, int maxDigits = 8)
        {
            var random = new Random();
            long result;
            
            do
            {
                int length = random.Next(minDigits, maxDigits + 1);
                long min = (long)Math.Pow(10, length - 1);
                long max = (long)Math.Pow(10, length) - 1;
                result = random.NextInt64(min, max);
            }
            while (await _accountBalanceTransactionGenericRepository.FindAll()
                .AnyAsync(a => a.OrderCode == result.ToString()));
    
            return result;
        }
    }
}
