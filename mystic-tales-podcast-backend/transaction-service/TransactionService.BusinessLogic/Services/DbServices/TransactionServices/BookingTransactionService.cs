using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompleteBookingTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransaction;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Helpers.DateHelpers;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities.SqlServer;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Infrastructure.Models.Kafka;
using TransactionService.Infrastructure.Services.Kafka;

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
        public BookingTransactionService(
            AppDbContext appDbContext,
            ILogger<BookingTransactionService> logger,
            IGenericRepository<BookingTransaction> bookingTransactionGenericRepository,
            IPayosConfig payosConfig,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            DateHelper dateHelper)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _bookingTransactionGenericRepository = bookingTransactionGenericRepository;
            _payosConfig = payosConfig;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _dateHelper = dateHelper;
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
                        case 3:
                            var depositBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(depositBookingTransaction);
                            break;
                        case 4:
                            var depositRefundBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(depositRefundBookingTransaction);
                            break;
                        case 5:
                            var depositCompensationBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(depositCompensationBookingTransaction);
                            break;
                        case 6:
                            var payTheRestBookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(payTheRestBookingTransaction);
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
                    if (bookingTransaction != null)
                    {
                        throw new Exception($"No booking transaction found for Id: {parameter.BookingTransactionId}");
                    }
                    if(bookingTransaction.TransactionStatusId != 1)
                    {
                        throw new Exception($"This booking transaction is not eligible for completion");
                    }
                    bookingTransaction.TransactionStatusId = 2;
                    var newBookingTransaction = await _bookingTransactionGenericRepository.UpdateAsync(bookingTransaction.Id, bookingTransaction);

                    await transaction.CommitAsync();
                    //Cách cổ điển: Sao chép toàn bộ RequestData rồi thêm thuộc tính mới
                    var newResponseData = command.RequestData;
                    newResponseData["BookingTransactionId"] = newBookingTransaction.Id;
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
    }
}
