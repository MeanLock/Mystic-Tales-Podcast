using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.Attributes;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.AccountBalanceCreatePaymentLink;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompleteBookingTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompleteMemberSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompletePodcastSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPayment;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPaymentRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateAccountBalanceTransactionRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransactionRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateMemberSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateMemberSubscriptionTransactionRollback;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreatePodcastSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreatePodcastSubscriptionTransactionRollback;
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
        [MessageHandler("create-member-subscription-transaction", "payment-processing-domain")]
        public async Task HandleCreateMemberSubscriptionTransactionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreateMemberSubscriptionTransactionParameterDTO>();
                    await _memberSubscriptionService.CreateMemberSubscriptionTransactionAsync(parameters, command);
                    _logger.LogInformation("Handled create-member-subscription-transaction command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-member-subscription-transaction.failed"
            );
        }
        [MessageHandler("complete-member-subscription-transaction", "payment-processing-domain")]
        public async Task HandleCompleteMemberSubscriptionTransactionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CompleteMemberSubscriptionTransactionParameterDTO>();
                    await _memberSubscriptionService.CompleteMemberSubscriptionTransactionAsync(parameters, command);
                    _logger.LogInformation("Handled complete-member-subscription-transaction command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "complete-member-subscription-transaction.failed"
            );
        }
        [MessageHandler("send-transaction-service-email", "payment-processing-domain")]
        public async Task HandleSendTransactionServiceEmailCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    // Placeholder for future implementation
                    _logger.LogInformation("Handled send-transaction-service-email command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "send-transaction-service-email.failed"
            );
        }
        [MessageHandler("confirm-payment-rollback", "payment-processing-domain")]
        public async Task HandleConfirmPaymentRollbackCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<ConfirmPaymentRollbackParameterDTO>();
                    await _accountBalanceTransactionService.ConfirmAccountBalanceTransactionPaymentRollbackAsync(parameters, command);
                    _logger.LogInformation("Handled confirm-payment-rollback command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "confirm-payment-rollback.failed"
            );
        }
        [MessageHandler("create-account-balance-transaction-rollback", "payment-processing-domain")]
        public async Task HandleCreateAccountBalanceTransactionRollbackCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreateAccountBalanceTransactionRollbackParameterDTO>();
                    await _accountBalanceTransactionService.CreateAccountBalanceTransactionRollbackAsync(parameters, command);
                    _logger.LogInformation("Handled create-account-balance-transaction-rollback command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-account-balance-transaction-rollback.failed"
            );
        }
        [MessageHandler("create-booking-transaction-rollback", "payment-processing-domain")]
        public async Task HandleCreateBookingTransactionRollbackCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreateBookingTransactionRollbackParameterDTO>();
                    await _bookingTransactionService.CreateBookingTransactionRollbackAsync(parameters, command);
                    _logger.LogInformation("Handled create-booking-transaction-rollback command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-booking-transaction-rollback.failed"
            );
        }
        [MessageHandler("create-member-subscription-transaction-rollback", "payment-processing-domain")]
        public async Task HandleCreateMemberSubscriptionTransactionRollbackCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreateMemberSubscriptionTransactionRollbackParameterDTO>();
                    await _memberSubscriptionService.CreateMemberSubscriptionTransactionRollbackAsync(parameters, command);
                    _logger.LogInformation("Handled create-member-subscription-transaction-rollback command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-member-subscription-transaction-rollback.failed"
            );
        }
        [MessageHandler("create-podcast-subscription-transaction-rollback", "payment-processing-domain")]
        public async Task HandleCreatePodcastSubscriptionTransactionRollbackCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreatePodcastSubscriptionTransactionRollbackParameterDTO>();
                    await _podcastSubscriptionService.CreatePodcastSubscriptionTransactionRollbackAsync(parameters, command);
                    _logger.LogInformation("Handled create-podcast-subscription-transaction-rollback command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "payment-processing-domain",
                failedEmitMessage: "create-podcast-subscription-transaction-rollback.failed"
            );
        }
    }
}
