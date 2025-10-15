using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace TransactionService.BusinessLogic.Services.DbServices
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
        public async Task CreateBookingTransaction(CreateBookingTransactionParameterDTO parameter, SagaCommandMessage command)
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
                            var bookingTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = null,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newBookingTransaction = await _bookingTransactionGenericRepository.CreateAsync(bookingTransaction);
                            break;
                        case 4:
                            var payoutTransaction = new BookingTransaction
                            {
                                BookingId = parameter.BookingId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            break;
                        default:
                            throw new Exception("Unsupported transaction type for booking transaction: " + transactionTypeId);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject{
                        { "BookingTransactionId", newBookingTransaction.Id },
                        { "CreatedAt", newBookingTransaction.CreatedAt}
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
    }
}
