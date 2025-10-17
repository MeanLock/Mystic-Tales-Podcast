using Microsoft.Extensions.Logging;
using SubscriptionService.BusinessLogic.Attributes;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription;
using SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.Infrastructure.Services.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.MessageHandlers
{
    public class SubscriptionManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly ILogger<SubscriptionManagementDomainMessageHandler> _logger;
        private readonly PodcastSubscriptionService _podcastSubscriptionService;
        public SubscriptionManagementDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<SubscriptionManagementDomainMessageHandler> logger,
            PodcastSubscriptionService podcastSubscriptionService) : base(messagingService, kafkaProducerService, logger)
        {
            _logger = logger;
            _podcastSubscriptionService = podcastSubscriptionService;
        }
        [MessageHandler("create-podcast-subscription", "subscription-management-domain")]
        public async Task HandleCreatePodcastSubscriptionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<CreatePodcastSubscriptionParameterDTO>();
                    await _podcastSubscriptionService.CreatePodcastSubscriptionAsync(parameters, command);
                    _logger.LogInformation("Handled create-podcast-subscription command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "subscription-management-domain",
                failedEmitMessage: "create-podcast-subscription.failed"
            );
        }
    }
}
