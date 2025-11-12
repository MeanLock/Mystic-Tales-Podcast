using Microsoft.Extensions.Logging;
using SubscriptionService.BusinessLogic.Attributes;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.ActivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscriptionRegistration;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelShowSubscriptionDmcaRemoveShowForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelShowSubscriptionShowDeletionForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelShowSubscriptionUnpublishShowForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateAccountPodcastSubscriptionRegistration;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateMemberSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeactivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeletePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.UpdatePodcastSubscription;
using SubscriptionService.BusinessLogic.Enums.Kafka;
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
        private readonly MemberSubscriptionService _memberSubscriptionService;
        private const string SAGA_TOPIC = KafkaTopicEnum.SubscriptionManagementDomain;
        public SubscriptionManagementDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<SubscriptionManagementDomainMessageHandler> logger,
            PodcastSubscriptionService podcastSubscriptionService,
            MemberSubscriptionService memberSubscriptionService) : base(messagingService, kafkaProducerService, logger)
        {
            _logger = logger;
            _podcastSubscriptionService = podcastSubscriptionService;
            _memberSubscriptionService = memberSubscriptionService;
        }
        [MessageHandler("create-podcast-subscription", SAGA_TOPIC)]
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
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-podcast-subscription.failed"
            );
        }
        [MessageHandler("update-podcast-subscription", SAGA_TOPIC)]
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
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-podcast-subscription.failed"
            );
        }
        [MessageHandler("delete-podcast-subscription", SAGA_TOPIC)]
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
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-podcast-subscription.failed"
            );
        }
        [MessageHandler("create-account-podcast-subscription-registration", SAGA_TOPIC)]
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
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-account-podcast-subscription-registration.failed"
            );
        }
        [MessageHandler("activate-podcast-subscription", SAGA_TOPIC)]
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
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "activate-podcast-subscription.failed"
            );
        }
        [MessageHandler("deactivate-podcast-subscription", SAGA_TOPIC)]
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
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "deactivate-podcast-subscription.failed"
            );
        }
        [MessageHandler("cancel-podcast-subscription-registration", SAGA_TOPIC)]
        public async Task HandleCancelPodcastSubscriptionRegistrationCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CancelPodcastSubscriptionRegistrationParameterDTO>();
                    await _podcastSubscriptionService.CancelPodcastSubscriptionRegistrationAsync(parameter, command);
                    _logger.LogInformation("Handled cancel-podcast-subscription-registration command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-podcast-subscription-registration.failed"
            );
        }
        [MessageHandler("cancel-podcast-subscription", SAGA_TOPIC)]
        public async Task HandlerCancelPodcastSubscriptionAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CancelPodcastSubscriptionParameterDTO>();
                    await _podcastSubscriptionService.CancelPodcastSubscriptionAsync(parameter, command);
                    _logger.LogInformation("Handled cancel-podcast-subscription command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-podcast-subscription.failed"
            );
        }
        [MessageHandler("create-member-subscription", SAGA_TOPIC)]
        public async Task HandleCreateMemberSubscriptionAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CreateMemberSubscriptionParameterDTO>();
                    await _memberSubscriptionService.CreateMemberSubscriptionAsync(parameter, command);
                    _logger.LogInformation("Handled create-member-subscriptio command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-member-subscriptio.failed"
            );
        }
        [MessageHandler("cancel-show-subscription-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleCancelShowSubscriptionDmcaRemoveShowForceCommandAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CancelShowSubscriptionDmcaRemoveShowForceParameterDTO>();
                    await _podcastSubscriptionService.CancelShowSubscriptionDmcaRemoveShowForceAsync(parameter, command);
                    _logger.LogInformation("Handled cancel-show-subscription-dmca-remove-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-show-subscription-dmca-remove-show-force.failed"
            );
        }
        [MessageHandler("cancel-show-subscription-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleCancelShowSubscriptionUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CancelShowSubscriptionUnpublishShowForceParameterDTO>();
                    await _podcastSubscriptionService.CancelShowSubscriptionUnpublishShowForceAsync(parameter, command);
                    _logger.LogInformation("Handled cancel-show-subscription-unpublish-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-show-subscription-unpublish-show-force.failed"
            );
        }
        [MessageHandler("cancel-channel-subscription-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleCancelChannelSubscriptionUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CancelChannelSubscriptionUnpublishChannelForceParameterDTO>();
                    await _podcastSubscriptionService.CancelChannelSubscriptionUnpublishChannelForceAsync(parameter, command);
                    _logger.LogInformation("Handled cancel-channel-subscription-unpublish-channel-force command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-channel-subscription-unpublish-channel-force.failed"
            );
        }
        [MessageHandler("cancel-show-subscription-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleCancelShowSubscriptionShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson,
                async (command) =>
                {
                    var parameter = command.RequestData.ToObject<CancelShowSubscriptionShowDeletionForceParameterDTO>();
                    await _podcastSubscriptionService.CancelShowSubscriptionShowDeletionForceAsync(parameter, command);
                    _logger.LogInformation("Handled cancel-show-subscription-show-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "cancel-show-subscription-show-deletion-force.failed"
            );
        }
    }
}
