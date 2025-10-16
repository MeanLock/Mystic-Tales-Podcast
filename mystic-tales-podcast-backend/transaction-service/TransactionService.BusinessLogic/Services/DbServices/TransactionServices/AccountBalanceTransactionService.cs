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
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.AccountBalanceCreatePaymentLink;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPayment;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPaymentRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateAccountBalanceTransactionRollback;
using TransactionService.BusinessLogic.DTOs.Transaction;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
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
        private readonly IPayosConfig _payosConfig;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        public AccountBalanceTransactionService(
            AppDbContext appDbContext, 
            ILogger<AccountBalanceTransactionService> logger,
            IGenericRepository<AccountBalanceTransaction> accountBalanceTransactionGenericRepository,
            IPayosConfig payosConfig,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _accountBalanceTransactionGenericRepository = accountBalanceTransactionGenericRepository;
            _payosConfig = payosConfig;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
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
                    long orderCode = GenerateRandomLong();

                    AccountBalanceTransaction accountBalanceTransaction = new AccountBalanceTransaction
                    {
                        AccountId = parameter.AccountId,
                        Amount = parameter.Amount,
                        OrderCode = orderCode.ToString(),
                        TransactionTypeId = 1,
                        TransactionStatusId = 1,
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
                        accountBalanceTransaction.TransactionStatusId = 2;
                    }
                    else
                    {
                        accountBalanceTransaction.TransactionStatusId = 4;
                    }

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
                    accountBalanceTransaction.TransactionStatusId = 4; // Thay đổi trạng thái giao dịch thành "Thất bại"
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
                    accountBalanceTransaction.TransactionStatusId = 4; // Thay đổi trạng thái giao dịch thành "Thất bại"
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
        public static long GenerateRandomLong(int minDigits = 5, int maxDigits = 8)
        {
            var random = new Random();
            int length = random.Next(minDigits, maxDigits + 1);
            long min = (long)Math.Pow(10, length - 1);
            long max = (long)Math.Pow(10, length) - 1;
            return random.NextInt64(min, max);
        }
    }
}
