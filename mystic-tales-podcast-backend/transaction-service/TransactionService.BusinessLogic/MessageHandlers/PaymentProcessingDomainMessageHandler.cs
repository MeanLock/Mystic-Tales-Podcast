using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.Attributes;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.AccountBalanceCreatePaymentLink;
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
        [MessageHandler("account-balance-create-payment-link", "payment-processing-domain")]
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
    }
}
