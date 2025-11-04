using Microsoft.Extensions.Logging;
using PodcastService.BusinessLogic.Attributes;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.AcceptEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardChannelEpisodesPublishReviewChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewDmcaRemoveEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardPodcasterEpisodesPublishReviewTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardShowEpisodesPublishReviewDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardShowEpisodesPublishReviewShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RejectEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequestEpisodeAudioExamination;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequireEpisodePublishReviewSessionEdit;
using PodcastService.BusinessLogic.Enums.Kafka;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Infrastructure.Services.Kafka;

namespace PodcastService.BusinessLogic.MessageHandlers
{
    public class ContentModerationDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly IMessagingService _messagingService;
        private readonly PodcastChannelService _podcastChannelService;
        private readonly PodcastShowService _podcastShowService;
        private readonly PodcastEpisodeService _podcastEpisodeService;
        private readonly ReviewSessionService _reviewSessionService;
        private readonly MailOperationService _mailOperationService;
        // private readonly AuthService _authService;
        private readonly KafkaProducerService _kafkaProducerService;
        private const string SAGA_TOPIC = KafkaTopicEnum.ContentModerationDomain;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;



        public ContentModerationDomainMessageHandler(
            IMessagingService messagingService,
            PodcastChannelService podcastChannelService,
            PodcastShowService podcastShowService,
            PodcastEpisodeService podcastEpisodeService,
            ReviewSessionService reviewSessionService,
            MailOperationService mailOperationService,

            KafkaProducerService kafkaProducerService,
            ILogger<ContentModerationDomainMessageHandler> logger,
            IMailPropertiesConfig mailPropertiesConfig) : base(messagingService, kafkaProducerService, logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _podcastChannelService = podcastChannelService;
            _podcastShowService = podcastShowService;
            _podcastEpisodeService = podcastEpisodeService;
            _reviewSessionService = reviewSessionService;

            _mailPropertiesConfig = mailPropertiesConfig;
        }

        #region Sample coding format must be followed
        #endregion

        [MessageHandler("request-episode-audio-examination", SAGA_TOPIC)]
        public async Task HandleRequestEpisodeAudioAutomaticExaminationAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<RequestEpisodeAudioExaminationParameterDTO>();
                    await _podcastEpisodeService.RequestPodcastEpisodeAudioExamination(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "request-episode-audio-examination.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("discard-episode-publish-review-session", SAGA_TOPIC)]
        public async Task HandleDiscardEpisodePublishReviewSessionAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<DiscardEpisodePublishReviewSessionParameterDTO>();
                    await _podcastEpisodeService.DiscardPodcastEpisodePublishReviewSession(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "discard-episode-publish-review-session.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("require-episode-publish-review-session-edit", SAGA_TOPIC)]
        public async Task HandleRequireEpisodePublishReviewSessionEditAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<RequireEpisodePublishReviewSessionEditParameterDTO>();
                    await _reviewSessionService.RequirePodcastEpisodePublishReviewSessionEdit(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "require-episode-publish-review-session-edit.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("accept-episode-publish-review-session", SAGA_TOPIC)]
        public async Task HandleAcceptEpisodePublishReviewSessionAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<AcceptEpisodePublishReviewSessionParameterDTO>();
                    await _reviewSessionService.AcceptPodcastEpisodePublishReviewSession(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "accept-episode-publish-review-session.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("reject-episode-publish-review-session", SAGA_TOPIC)]
        public async Task HandleRejectEpisodePublishReviewSessionAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<RejectEpisodePublishReviewSessionParameterDTO>();
                    await _reviewSessionService.RejectPodcastEpisodePublishReviewSession(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "reject-episode-publish-review-session.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("discard-episode-publish-review-dmca-remove-episode-force", SAGA_TOPIC)]
        public async Task HandleDiscardEpisodePublishReviewDmcaRemoveEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<DiscardEpisodePublishReviewDmcaRemoveEpisodeForceParameterDTO>();
                    await _podcastEpisodeService.DiscardPodcastEpisodePublishReviewDmcaRemoveEpisodeForce(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "discard-episode-publish-review-dmca-remove-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("discard-show-episodes-publish-review-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleDiscardShowEpisodesPublishReviewDmcaRemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<DiscardShowEpisodesPublishReviewDmcaRemoveShowForceParameterDTO>();
                    await _podcastEpisodeService.DiscardPodcastShowEpisodesPublishReviewDmcaRemoveShowForce(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "discard-show-episodes-publish-review-dmca-remove-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("discard-episode-publish-review-episode-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeListenSessionEpisodeDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<DiscardEpisodePublishReviewEpisodeDeletionForceParameterDTO>();
                    await _podcastEpisodeService.DiscardEpisodePublishReviewEpisodeDeletionForce(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-episode-listen-session-episode-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("discard-show-episodes-publish-review-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowEpisodesListenSessionShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    // var episode = command.RequestData.ToObject<DeleteShowEpisodesListenSessionShowDeletionForceParameterDTO>();
                    var episode = command.RequestData.ToObject<DiscardShowEpisodesPublishReviewShowDeletionForceParameterDTO>();
                    await _podcastEpisodeService.DiscardShowEpisodesPublishReviewShowDeletionForce(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-episodes-listen-session-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("discard-channel-episodes-publish-review-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelEpisodesListenSessionChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    // var episode = command.RequestData.ToObject<DiscardEpisodePublishReviewDmcaRemoveEpisodeForceParameterDTO?>();
                    var episode = command.RequestData.ToObject<DiscardChannelEpisodesPublishReviewChannelDeletionForceParameterDTO>();
                    await _podcastEpisodeService.DiscardChannelEpisodesPublishReviewChannelDeletionForce(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-episodes-listen-session-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("discard-podcaster-episodes-publish-review-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeletePodcasterEpisodesListenSessionTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<DiscardPodcasterEpisodesPublishReviewTerminatePodcasterForceParameterDTO>();
                    await _podcastEpisodeService.DiscardPodcasterEpisodesPublishReviewTerminatePodcasterForce(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-podcaster-episodes-listen-session-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }
    }
}

