using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.Attributes;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.AccountBalanceCreatePaymentLink;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPayment;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransaction;
using TransactionService.BusinessLogic.Services.DbServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.BusinessLogic.MessageHandlers
{
    public class PaymentProcessingDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly ILogger<PaymentProcessingDomainMessageHandler> _logger;
        private readonly AccountBalanceTransactionService _accountBalanceTransactionService;
        public PaymentProcessingDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<PaymentProcessingDomainMessageHandler> logger,
            AccountBalanceTransactionService accountBalanceTransactionService) : base(messagingService, kafkaProducerService, logger)
        {
            _logger = logger;
            _accountBalanceTransactionService = accountBalanceTransactionService;
        }
        [MessageHandler("create-payment-link", "payment-processing-domain")]
        public async Task HandleAccountBalanceCreatePaymentLinkCommandAsync(string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<AccountBalanceCreatePaymentLinkParameterDTO>();
                    await _accountBalanceTransactionService.CreateAccountBalanceTransactionDepositPaymentLink(parameters, command);
                    _logger.LogInformation("Handled account-balance-create-payment-link command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "saga-orchestrator-events",
                failedEmitMessage: "account-balance-create-payment-link.failed"
            );
        }
        [MessageHandler("confirm-payment", "payment-processing-domain")]
        public async Task HandleAccountBalanceConfirmPaymentCommandAsync(string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<ConfirmPaymentParameterDTO>();
                    await _accountBalanceTransactionService.ConfirmAccountBalanceTransactionPaymentAsync(parameters, command);
                    _logger.LogInformation("Handled account-balance-confirm-payment command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "saga-orchestrator-events",
                failedEmitMessage: "account-balance-confirm-payment.failed"
            );
        }
        [MessageHandler("create-withdrawal-request", "payment-processing-domain")] // Placeholder for future implementation
        public async Task HandleAccountBalanceCreateWithdrawalRequestCommandAsync(string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    // Placeholder for future implementation
                    _logger.LogInformation("Handled account-balance-create-withdrawal-request command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "saga-orchestrator-events",
                failedEmitMessage: "account-balance-create-withdrawal-request.failed"
            );
        }
        [MessageHandler("create-booking-transaction", "payment-processing-domain")]
        public async Task HandleBookingTransactionCreateBookingTransactionCommandAsync(string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreateBookingTransactionParameterDTO>();
                    await _accountBalanceTransactionService.CreateBookingTransaction(parameters, command);
                    _logger.LogInformation("Handled booking-transaction-create-booking-transaction command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "saga-orchestrator-events",
                failedEmitMessage: "booking-transaction-create-booking-transaction.failed"
            );
        }
    }
}
