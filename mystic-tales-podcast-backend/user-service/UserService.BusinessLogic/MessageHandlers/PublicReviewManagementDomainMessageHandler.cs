using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserService.BusinessLogic.Attributes;
using UserService.BusinessLogic.Enums.Kafka;
using UserService.BusinessLogic.Services.DbServices.UserServices;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.Infrastructure.Services.Kafka;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeletePodcastBuddyReview;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.UpdatePodcastBuddyReview;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.CreatePodcastBuddyReview;
using UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteBuddyReviewTerminatePodcasterForce;

namespace UserService.BusinessLogic.MessageHandlers
{
    public class PublicReviewManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly IMessagingService _messagingService;
        private readonly AccountService _accountService;
        private readonly AuthService _authService;
        private readonly KafkaProducerService _kafkaProducerService;
        private const string SAGA_TOPIC = KafkaTopicEnum.PublicReviewManagementDomain;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;



        public PublicReviewManagementDomainMessageHandler(
            IMessagingService messagingService,
            AccountService accountService,
            AuthService authService,
            KafkaProducerService kafkaProducerService,
            ILogger<PublicReviewManagementDomainMessageHandler> logger,
            IMailPropertiesConfig mailPropertiesConfig) : base(messagingService, kafkaProducerService, logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _accountService = accountService;
            _authService = authService;
            _mailPropertiesConfig = mailPropertiesConfig;
        }


        [MessageHandler("create-podcast-buddy-review", SAGA_TOPIC)]
        public async Task HandleCreatePodcastBuddyReviewAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createPodcastBuddyReviewParameterDTO = command.RequestData.ToObject<CreatePodcastBuddyReviewParameterDTO>();
                    await _accountService.CreatePodcastBuddyReview(createPodcastBuddyReviewParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-podcast-buddy-review.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-podcast-buddy-review", SAGA_TOPIC)]
        public async Task HandleUpdatePodcastBuddyReviewAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var updatePodcastBuddyReviewParameterDTO = command.RequestData.ToObject<UpdatePodcastBuddyReviewParameterDTO>();
                    await _accountService.UpdatePodcastBuddyReview(updatePodcastBuddyReviewParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-podcast-buddy-review.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-podcast-buddy-review", SAGA_TOPIC)]
        public async Task HandleDeletePodcastBuddyReviewAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deletePodcastBuddyReviewParameterDTO = command.RequestData.ToObject<DeletePodcastBuddyReviewParameterDTO>();
                    await _accountService.DeletePodcastBuddyReview(deletePodcastBuddyReviewParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-podcast-buddy-review.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-buddy-review-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeleteBuddyReviewTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deletePodcastBuddyReviewParameterDTO = command.RequestData.ToObject<DeleteBuddyReviewTerminatePodcasterForceParameterDTO>();
                    await _accountService.DeleteBuddyReviewTerminatePodcasterForce(deletePodcastBuddyReviewParameterDTO, command);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-buddy-review-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }
    }
}

