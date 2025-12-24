using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompleteBookingTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransactionRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ProcessingBookingTransaction;
using TransactionService.BusinessLogic.DTOs.SystemConfiguration;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Enums.Transaction;
using TransactionService.BusinessLogic.Helpers.DateHelpers;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities.SqlServer;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Infrastructure.Models.Kafka;
using TransactionService.Infrastructure.Services.Kafka;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TransactionService.BusinessLogic.Services.DbServices.TransactionServices
{
    public class BookingTransactionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<BookingTransactionService> _logger;
        private readonly IGenericRepository<BookingTransaction> _bookingTransactionGenericRepository;
        private readonly IPayosConfig _payosConfig;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly DateHelper _dateHelper;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public BookingTransactionService(
            AppDbContext appDbContext,
            ILogger<BookingTransactionService> logger,
            IGenericRepository<BookingTransaction> bookingTransactionGenericRepository,
            IPayosConfig payosConfig,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            DateHelper dateHelper,
            HttpServiceQueryClient httpServiceQueryClient)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _bookingTransactionGenericRepository = bookingTransactionGenericRepository;
            _payosConfig = payosConfig;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _dateHelper = dateHelper;
            _httpServiceQueryClient = httpServiceQueryClient;
        }
        public async Task CreateBookingTransactionAsync(CreateBookingTransactionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var transactionTypeId = parameter.TransactionTypeId;
                    var newBookingTransaction = new BookingTransaction();
                    switch (transactionTypeId)
                    {
                        case (int)TransactionTypeEnum.BookingDeposit:
                            var depositBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(depositBookingTransaction);
                            break;
                        case (int)TransactionTypeEnum.BookingDepositRefund:
                            var depositRefundBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(depositRefundBookingTransaction);
                            break;
                        case (int)TransactionTypeEnum.BookingDepositCompensation:
                            var depositCompensationBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(depositCompensationBookingTransaction);

                            if(newBookingTransaction.Profit != null && newBookingTransaction.Profit > 0)
                            {
                                var requestData = new JObject
                                {
                                    { "BookingId", newBookingTransaction.BookingId },
                                    { "Profit", null },
                                    { "AccountId", parameter.AccountId },
                                    { "PodcasterId", parameter.PodcasterId },
                                    { "Amount", newBookingTransaction.Profit },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.SystemBookingIncome }
                                };
                                var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(KafkaTopicEnum.PaymentProcessingDomain, requestData, null, "booking-system-payment-flow");
                                await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
                            }

                            break;
                        case (int)TransactionTypeEnum.BookingPayTheRest:
                            var config = await GetActiveSystemConfigProfile();
                            if (config == null || config.BookingConfig == null)
                            {
                                throw new Exception("Active system config profile or booking config not found.");
                            }
                            var profitRate = config.BookingConfig.ProfitRate;
                            var depositRate = config.BookingConfig.DepositRate;
                            var payTheRestBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(payTheRestBookingTransaction);

                            var originalPrice = parameter.Amount / (decimal)depositRate;
                            var requestData2 = new JObject
                            {
                                { "BookingId", parameter.BookingId },
                                { "Profit", originalPrice * (decimal)profitRate },
                                { "AccountId", parameter.AccountId },
                                { "PodcasterId", parameter.PodcasterId },
                                { "Amount", originalPrice - originalPrice * (decimal)profitRate },
                                { "TransactionTypeId", (int)TransactionTypeEnum.PodcasterBookingIncome }
                            };
                            var startSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(KafkaTopicEnum.PaymentProcessingDomain, requestData2, null, "booking-podcaster-payment-flow");
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage2);

                            break;
                        case (int)TransactionTypeEnum.PodcasterBookingIncome:
                            var additionalStoragePurchaseBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(additionalStoragePurchaseBookingTransaction);

                            var requestData3 = new JObject
                            {
                                { "BookingId", newBookingTransaction.BookingId },
                                { "Profit", null },
                                { "AccountId", parameter.AccountId },
                                { "PodcasterId", parameter.PodcasterId },
                                { "Amount", newBookingTransaction.Profit },
                                { "TransactionTypeId", (int)TransactionTypeEnum.SystemBookingIncome }
                            };
                            var startSagaTriggerMessage3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(KafkaTopicEnum.PaymentProcessingDomain, requestData3, null, "booking-system-payment-flow");
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage3);

                            break;
                        case (int)TransactionTypeEnum.SystemBookingIncome:
                            var systemBookingIncomeTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Success,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(systemBookingIncomeTransaction);
                            break;
                        default:
                            throw new Exception("Unsupported transaction type for booking transaction: " + transactionTypeId);
                    }

                    await transaction.CommitAsync();
                    var newRequestData = command.RequestData;
                    newRequestData["BookingTransactionId"] = newBookingTransaction.Id;

                    var newResponseData = new JObject{
                        { "BookingTransactionId", newBookingTransaction.Id },
                        { "CreatedAt", newBookingTransaction.CreatedAt}
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
                    _logger.LogInformation("Create booking transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while create booking transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create booking transaction failed, error: " + ex.Message }
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
                    _logger.LogInformation("Create booking transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CompleteBookingTransactionAsync(CompleteBookingTransactionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var bookingTransaction = await _bookingTransactionGenericRepository.FindByIdAsync(parameter.BookingTransactionId);
                    if (bookingTransaction == null)
                    {
                        throw new Exception($"No booking transaction found for Id: {parameter.BookingTransactionId}");
                    }
                    if(bookingTransaction.TransactionStatusId != (int)TransactionStatusEnum.Pending)
                    {
                        throw new Exception($"This booking transaction is not eligible for completion");
                    }
                    bookingTransaction.TransactionStatusId = (int)TransactionStatusEnum.Success;
                    var newBookingTransaction = await _bookingTransactionGenericRepository.UpdateAsync(bookingTransaction.Id, bookingTransaction);

                    await transaction.CommitAsync();
                    //Cách cổ điển: Sao chép toàn bộ RequestData rồi thêm thuộc tính mới
                    var newResponseData = command.RequestData;
                    newResponseData["UpdatedAt"] = newBookingTransaction.UpdatedAt;

                    //Cách 1: Sử dụng Merge (nếu không có thuộc tính trùng tên)
                    //var newResponseData = new JObject{
                    //    { "BookingTransactionId", newBookingTransaction.Id },
                    //    { "UpdatedAt", newBookingTransaction.UpdatedAt}
                    //};
                    //// Merge all properties from RequestData into newResponseData
                    //newResponseData.Merge(command.RequestData);

                    //Cách 2: Dùng vòng lặp để thêm từng thuộc tính (nếu có thuộc tính trùng tên)
                    //var newResponseData = new JObject{
                    //    { "BookingTransactionId", newBookingTransaction.Id },
                    //    { "UpdatedAt", newBookingTransaction.UpdatedAt}
                    //};
                    // // Add all properties from RequestData
                    //foreach (var property in command.RequestData.Properties())
                    //{
                    //    newResponseData[property.Name] = property.Value;
                    //}

                    //Cách 3: Clone toàn bộ RequestData rồi thêm thuộc tính mới (nếu có thuộc tính trùng tên)
                    // // Clone RequestData to avoid modifying the original
                    //var newResponseData = (JObject)command.RequestData.DeepClone();
                    // // Add your specific properties
                    //newResponseData["BookingTransactionId"] = newBookingTransaction.Id;
                    //newResponseData["UpdatedAt"] = newBookingTransaction.UpdatedAt;

                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Completing booking transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while completing booking transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Completing booking transaction failed, error: " + ex.Message }
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
                    _logger.LogInformation("Completing booking transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        //public async Task ProcessingBookingTransactionAsync(ProcessingBookingTransactionParameterDTO parameter, SagaCommandMessage command)
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var messageName = command.MessageName;
        //            var sagaId = command.SagaInstanceId;
        //            var flowName = command.FlowName;
        //            var responseData = command.LastStepResponseData;


        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while Processing Booking Transaction for SagaId: {SagaId}", command.SagaInstanceId);
        //            var newResponseData = new JObject{
        //                    { "ErrorMessage", "Processing Booking Transaction failed, error: " + ex.Message }
        //                };
        //            var newMessageName = command.MessageName + ".failed";
        //            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                topic: KafkaTopicEnum.PaymentProcessingDomain,
        //                requestData: command.RequestData,
        //                responseData: newResponseData,
        //                sagaInstanceId: command.SagaInstanceId,
        //                flowName: command.FlowName,
        //                messageName: newMessageName);
        //            await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
        //            _logger.LogInformation("Processing Booking Transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
        //        }
        //    }
        //}
        public async Task CreateBookingTransactionRollbackAsync(CreateBookingTransactionRollbackParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    BookingTransaction? bookingTransaction = _bookingTransactionGenericRepository.FindByIdAsync(parameter.BookingTransactionId).Result;
                    if (bookingTransaction == null)
                    {
                        throw new Exception("Booking Transaction not found.");
                    }
                    ;
                    bookingTransaction.TransactionStatusId = (int)TransactionStatusEnum.Error; // Thay đổi trạng thái giao dịch thành "Thất bại"
                    await _bookingTransactionGenericRepository.UpdateAsync(bookingTransaction.Id, bookingTransaction);
                    await transaction.CommitAsync();
                    var newResponseData = new JObject{
                        { "BookingTransactionId", bookingTransaction.Id },
                        { "TransactionStatusId", bookingTransaction.TransactionStatusId}
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
                    _logger.LogInformation("Rollback create booking transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while rolling back create booking transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Rollback create booking transactionnt failed, error: " + ex.Message }
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
                    _logger.LogInformation("Rollback create booking transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        private async Task<SystemConfigProfileDTO?> GetActiveSystemConfigProfile()
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeSystemConfigProfile",
                            QueryType = "findall",
                            EntityType = "SystemConfigProfile",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsActive = true
                                    },
                                    include = "AccountConfig,AccountViolationLevelConfigs, BookingConfig, PodcastSubscriptionConfigs, PodcastSuggestionConfig, ReviewSessionConfig",

                                }),
                            Fields = new[] { "Id", "Name", "IsActive", "AccountConfig", "AccountViolationLevelConfigs", "BookingConfig", "PodcastSubscriptionConfigs", "PodcastSuggestionConfig", "ReviewSessionConfig" }
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

                return result.Results?["activeSystemConfigProfile"] is JArray configArray && configArray.Count > 0
                    ? configArray.First.ToObject<SystemConfigProfileDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while fetching active system config profile");
                throw new HttpRequestException("Failed to fetch active system config profile. error: " + ex.Message);
            }
        }
    }
}
