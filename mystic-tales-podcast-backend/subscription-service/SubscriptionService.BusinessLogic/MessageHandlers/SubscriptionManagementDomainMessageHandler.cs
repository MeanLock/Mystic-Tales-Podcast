using Microsoft.Extensions.Logging;
using SubscriptionService.BusinessLogic.Attributes;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.ActivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateAccountPodcastSubscriptionRegistration;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeactivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeletePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.UpdatePodcastSubscription;
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
        [MessageHandler("update-podcast-subscription", "subscription-management-domain")]
        public async Task HandleUpdatePodcastSubscriptionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameters = command.RequestData.ToObject<UpdatePodcastSubscriptionParameterDTO>();
                    await _podcastSubscriptionService.UpdatePodcastSubscriptionAsync(parameters, command);
                    _logger.LogInformation("Handled update-podcast-subscription command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "subscription-management-domain",
                failedEmitMessage: "update-podcast-subscription.failed"
            );
        }
        [MessageHandler("delete-podcast-subscription", "subscription-management-domain")]
        public async Task HandleDeletePodcastSubscriptionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<DeletePodcastSubscriptionParameterDTO>();
                    await _podcastSubscriptionService.DeletePodcastSubscriptionAsync(parameter, command);
                    _logger.LogInformation("Handled delete-podcast-subscription command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "subscription-management-domain",
                failedEmitMessage: "delete-podcast-subscription.failed"
            );
        }
        [MessageHandler("create-account-podcast-subscription-registration", "subscription-management-domain")]
        public async Task HandleCreateAccountPodcastSubscriptionRegistrationCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CreateAccountPodcastSubscriptionRegistrationParameterDTO>();
                    await _podcastSubscriptionService.CreateAccountPodcastSubscriptionRegistrationAsync(parameter, command);
                    _logger.LogInformation("Handled create-account-podcast-subscription-registration command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "subscription-management-domain",
                failedEmitMessage: "create-account-podcast-subscription-registration.failed"
            );
        }
        [MessageHandler("activate-podcast-subscription", "subscription-management-domain")]
        public async Task HandleActivatePodcastSubscriptionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<ActivatePodcastSubscriptionParameterDTO>();
                    await _podcastSubscriptionService.ActivatePodcastSubscriptionAsync(parameter, command);
                    _logger.LogInformation("Handled activate-podcast-subscription command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "subscription-management-domain",
                failedEmitMessage: "activate-podcast-subscription.failed"
            );
        }
        [MessageHandler("deactivate-podcast-subscription", "subscription-management-domain")]
        public async Task HandleDeactivatePodcastSubscriptionCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<DeactivatePodcastSubscriptionParameterDTO>();
                    await _podcastSubscriptionService.DeactivatePodcastSubscriptionAsync(parameter, command);
                    _logger.LogInformation("Handled deactivate-podcast-subscription command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: "subscription-management-domain",
                failedEmitMessage: "deactivate-podcast-subscription.failed"
            );
        }
    }
}
