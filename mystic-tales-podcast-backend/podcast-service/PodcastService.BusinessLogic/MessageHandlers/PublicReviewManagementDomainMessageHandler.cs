using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PodcastService.BusinessLogic.Attributes;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.CreateShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteChannelShowsReviewChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteChannelShowsReviewUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeletePodcasterShowsReviewTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReviewDMCARemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReviewShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReviewUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.UpdateShowReview;
using PodcastService.BusinessLogic.Enums.Kafka;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Infrastructure.Services.Kafka;

namespace PodcastService.BusinessLogic.MessageHandlers
{
    public class PublicReviewManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly IMessagingService _messagingService;
        private readonly PodcastShowService _podcastShowService;
        private readonly KafkaProducerService _kafkaProducerService;
        private const string SAGA_TOPIC = KafkaTopicEnum.PublicReviewManagementDomain;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;



        public PublicReviewManagementDomainMessageHandler(
            IMessagingService messagingService,
            PodcastShowService podcastShowService,
            KafkaProducerService kafkaProducerService,
            ILogger<PublicReviewManagementDomainMessageHandler> logger,
            IMailPropertiesConfig mailPropertiesConfig) : base(messagingService, kafkaProducerService, logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _podcastShowService = podcastShowService;
            _mailPropertiesConfig = mailPropertiesConfig;
        }




        // [MessageHandler("create-podcast-buddy-review", SAGA_TOPIC)]
        // public async Task HandleCreatePodcastBuddyReviewAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var createPodcastBuddyReviewParameterDTO = command.RequestData.ToObject<CreatePodcastBuddyReviewParameterDTO>();
        //             Console.WriteLine("Received CreatePodcastBuddyReviewParameterDTO: " + command.FlowName);
        //             await _accountService.CreatePodcastBuddyReview(createPodcastBuddyReviewParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "create-podcast-buddy-review.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("update-podcast-buddy-review", SAGA_TOPIC)]
        // public async Task HandleUpdatePodcastBuddyReviewAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var updatePodcastBuddyReviewParameterDTO = command.RequestData.ToObject<UpdatePodcastBuddyReviewParameterDTO>();
        //             Console.WriteLine("Received UpdatePodcastBuddyReviewParameterDTO: " + command.FlowName);
        //             await _accountService.UpdatePodcastBuddyReview(updatePodcastBuddyReviewParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "update-podcast-buddy-review.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("delete-podcast-buddy-review", SAGA_TOPIC)]
        // public async Task HandleDeletePodcastBuddyReviewAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var deletePodcastBuddyReviewParameterDTO = command.RequestData.ToObject<DeletePodcastBuddyReviewParameterDTO>();
        //             await _accountService.DeletePodcastBuddyReview(deletePodcastBuddyReviewParameterDTO, command);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "delete-podcast-buddy-review.failed"    // From YAML onFailure.emit
        //     );
        // }

        [MessageHandler("create-show-review", SAGA_TOPIC)]
        public async Task HandleCreateShowReviewAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createPodcastShowReviewParameterDTO = command.RequestData.ToObject<CreateShowReviewParameterDTO>();
                    await _podcastShowService.CreatePodcastShowReview(createPodcastShowReviewParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-show-review.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-show-review", SAGA_TOPIC)]
        public async Task HandleUpdateShowReviewAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var updatePodcastShowReviewParameterDTO = command.RequestData.ToObject<UpdateShowReviewParameterDTO>();
                    await _podcastShowService.UpdatePodcastShowReview(updatePodcastShowReviewParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-show-review.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-review", SAGA_TOPIC)]
        public async Task HandleDeleteShowReviewAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deletePodcastShowReviewParameterDTO = command.RequestData.ToObject<DeleteShowReviewParameterDTO>();
                    await _podcastShowService.DeletePodcastShowReview(deletePodcastShowReviewParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-review.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-review-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowReviewDMCARemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteShowReviewDMCARemoveShowForceParameterDTO = command.RequestData.ToObject<DeleteShowReviewDMCARemoveShowForceParameterDTO>();
                    await _podcastShowService.DeleteShowReviewDMCARemoveShowForce(deleteShowReviewDMCARemoveShowForceParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-review-dmca-remove-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-review-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowReviewUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var show = command.RequestData.ToObject<DeleteShowReviewUnpublishShowForceParameterDTO>();
                    await _podcastShowService.DeletePodcastShowReviewUnpublishShowForce(show, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-review-unpublish-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-channel-shows-review-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelShowsReviewUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteChannelShowsReviewUnpublishChannelForceParameterDTO>();
                    await _podcastShowService.DeleteChannelShowsReviewUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-shows-review-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-review-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowReviewShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var show = command.RequestData.ToObject<DeleteShowReviewShowDeletionForceParameterDTO>();
                    await _podcastShowService.DeletePodcastShowReviewShowDeletionForce(show, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-review-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-channel-shows-review-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelShowsReviewChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteChannelShowsReviewChannelDeletionForceParameterDTO>();
                    await _podcastShowService.DeleteChannelShowsReviewChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-shows-review-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-podcaster-shows-review-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeletePodcasterShowsReviewTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeletePodcasterShowsReviewTerminatePodcasterForceParameterDTO>();
                    await _podcastShowService.DeletePodcasterShowsReviewTerminatePodcasterForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-podcaster-shows-review-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }
    }
}

