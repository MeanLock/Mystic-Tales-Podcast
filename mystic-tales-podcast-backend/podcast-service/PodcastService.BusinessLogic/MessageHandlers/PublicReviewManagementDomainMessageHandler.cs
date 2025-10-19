using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PodcastService.BusinessLogic.Attributes;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.CreateShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.UpdateShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcastBuddyReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeletePodcastBuddyReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdatePodcastBuddyReview;
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
    }
}

