using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.Attributes;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.AccountBalanceCreatePaymentLink;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompleteBookingTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompletePodcastSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPayment;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreatePodcastSubscriptionTransaction;
using TransactionService.BusinessLogic.Services.DbServices.TransactionServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.BusinessLogic.MessageHandlers
{
    public class PaymentProcessingDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly ILogger<PaymentProcessingDomainMessageHandler> _logger;
        private readonly AccountBalanceTransactionService _accountBalanceTransactionService;
        private readonly BookingTransactionService _bookingTransactionService;
        private readonly PodcastSubscriptionService _podcastSubscriptionService;
        private readonly MemberSubscriptionService _memberSubscriptionService;
        public PaymentProcessingDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<PaymentProcessingDomainMessageHandler> logger,
            AccountBalanceTransactionService accountBalanceTransactionService,
            BookingTransactionService bookingTransactionService,
            PodcastSubscriptionService podcastSubscriptionService,
            MemberSubscriptionService memberSubscriptionService) : base(messagingService, kafkaProducerService, logger)
        {
            _logger = logger;
            _accountBalanceTransactionService = accountBalanceTransactionService;
            _bookingTransactionService = bookingTransactionService;
            _podcastSubscriptionService = podcastSubscriptionService;
            _memberSubscriptionService = memberSubscriptionService;
        }
        [MessageHandler("create-payment-link", "payment-processing-domain")]
        public async Task HandleCreatePaymentLinkCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<AccountBalanceCreatePaymentLinkParameterDTO>();
                    await _accountBalanceTransactionService.CreateAccountBalanceTransactionDepositPaymentLinkAsync(parameters, command);
                    _logger.LogInformation("Handled create-payment-link command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-payment-link.failed"
            );
        }
        [MessageHandler("confirm-payment", "payment-processing-domain")]
        public async Task HandleConfirmPaymentCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<ConfirmPaymentParameterDTO>();
                    await _accountBalanceTransactionService.ConfirmAccountBalanceTransactionPaymentAsync(parameters, command);
                    _logger.LogInformation("Handled confirm-payment command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "confirm-payment.failed"
            );
        }
        [MessageHandler("create-withdrawal-request", "payment-processing-domain")] // Placeholder for future implementation
        public async Task HandleCreateWithdrawalRequestCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    // Placeholder for future implementation
                    _logger.LogInformation("Handled create-withdrawal-request command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-withdrawal-request.failed"
            );
        }
        [MessageHandler("create-booking-transaction", "payment-processing-domain")]
        public async Task HandleCreateBookingTransactionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreateBookingTransactionParameterDTO>();
                    await _bookingTransactionService.CreateBookingTransactionAsync(parameters, command);
                    _logger.LogInformation("Handled create-booking-transaction command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-booking-transaction.failed"
            );
        }
        [MessageHandler("complete-booking-transaction", "payment-processing-domain")]
        public async Task HandleCompleteBookingTransaction(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CompleteBookingTransactionParameterDTO>();
                    await _bookingTransactionService.CompleteBookingTransactionAsync(parameters, command);
                    _logger.LogInformation("Handled complete-booking-transaction command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "complete-booking-transaction.failed"
            );
        }
        [MessageHandler("create-podcast-subscription-transaction", "payment-processing-domain")]
        public async Task HandleCreatePodcastSubscriptionTransactionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreatePodcastSubscriptionTransactionParameterDTO>();
                    await _podcastSubscriptionService.CreatePodcastSubscriptionTransactionAsync(parameters, command);
                    _logger.LogInformation("Handled create-podcast-subscription-transaction command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-podcast-subscription-transaction.failed"
            );
        }
        [MessageHandler("complete-podcast-subscription-transaction", "payment-processing-domain")]
        public async Task HandleCompletePodcastSubscriptionTransactionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CompletePodcastSubscriptionTransactionParameterDTO>();
                    await _podcastSubscriptionService.CompletePodcastSubscriptionTransactionAsync(parameters, command);
                    _logger.LogInformation("Handled complete-podcast-subscription-transaction command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "complete-podcast-subscription-transaction.failed"
            );
        }
    }
}
