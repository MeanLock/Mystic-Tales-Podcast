using Microsoft.Extensions.Logging;
using PodcastService.BusinessLogic.Attributes;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.AssignShowChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CompleteAllUserEpisodeListenSessions;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateBackgroundSoundTrack;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteBackgroundSoundTrack;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteChannelChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteChannelEpisodesChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteChannelShowsChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteEpisodeEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteShowEpisodesShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteShowShowShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.KeepChannelShowsChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusChannelTotalFavorite;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusShowTotalFollow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodeDraftAudio;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodePublishAudio;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedEpisodesDmcaChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedEpisodesDmcaUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedShowsDmcaChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedShowsDmcaUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelEpisodesListenSessionContentChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelEpisodesListenSessionContentUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaUnpublishEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowEpisodesDmcaShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowEpisodesDmcaUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeDmcaRemoveEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeListenSessionContentDmcaRemoveEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeListenSessionContentEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeListenSessionContentUnpublishEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemovePodcasterEpisodesListenSessionContentTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowDmcaDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesListenSessionContentDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesListenSessionContentShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesListenSessionContentUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RestoreContentDmca;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitEpisodeAudioFile;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitShowTrailerAudioFile;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractChannelTotalFavorite;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractShowTotalFollow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.TakedownContentDmca;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishChannelUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishEpisodeUnpublishEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishPodcasterChannelsTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishPodcasterEpisodesTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishPodcasterShowsTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishShowUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateBackgroundSoundTrack;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisodeListenSessionDuration;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UploadEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendPodcastServiceEmail;
using PodcastService.BusinessLogic.Enums.Kafka;
using PodcastService.BusinessLogic.Models.Mail;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Infrastructure.Services.Kafka;

namespace PodcastService.BusinessLogic.MessageHandlers
{
    public class ContentManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly IMessagingService _messagingService;
        private readonly PodcastChannelService _podcastChannelService;
        private readonly PodcastShowService _podcastShowService;
        private readonly PodcastEpisodeService _podcastEpisodeService;
        private readonly PodcastBackgroundSoundTrackService _podcastBackgroundSoundTrackService;
        private readonly MailOperationService _mailOperationService;
        // private readonly AuthService _authService;
        private readonly KafkaProducerService _kafkaProducerService;
        private const string SAGA_TOPIC = KafkaTopicEnum.ContentManagementDomain;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;



        public ContentManagementDomainMessageHandler(
            IMessagingService messagingService,
            PodcastChannelService podcastChannelService,
            PodcastShowService podcastShowService,
            PodcastEpisodeService podcastEpisodeService,
            PodcastBackgroundSoundTrackService podcastBackgroundSoundTrackService,
            MailOperationService mailOperationService,

            KafkaProducerService kafkaProducerService,
            ILogger<ContentManagementDomainMessageHandler> logger,
            IMailPropertiesConfig mailPropertiesConfig) : base(messagingService, kafkaProducerService, logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
            _podcastChannelService = podcastChannelService;
            _podcastShowService = podcastShowService;
            _podcastEpisodeService = podcastEpisodeService;
            _podcastBackgroundSoundTrackService = podcastBackgroundSoundTrackService;

            _mailPropertiesConfig = mailPropertiesConfig;
        }

        #region Sample coding format must be followed
        #endregion


        [MessageHandler("send-user-service-email", SAGA_TOPIC)]
        public async Task HandleSendPodcastServiceEmailAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var sendPodcastServiceEmailParameterDTO = command.RequestData.ToObject<SendPodcastServiceEmailParameterDTO>();
                    var mailInfo = sendPodcastServiceEmailParameterDTO.SendPodcastServiceEmailMailInfo;
                    Console.WriteLine("Preparing to send email of type: " + mailInfo.MailTypeName);
                    object mailModel = mailInfo.MailTypeName switch
                    {
                        "CustomerRegistrationVerification" => mailInfo.MailObject.ToObject<CustomerRegistrationVerificationMailViewModel>(),
                        "CustomerPasswordReset" => mailInfo.MailObject.ToObject<CustomerPasswordResetMailViewModel>(),
                        "PodcasterRequestConfirmation" => mailInfo.MailObject.ToObject<PodcasterRequestConfirmationMailViewModel>(),
                        "PodcasterRequestResult" => mailInfo.MailObject.ToObject<PodcasterRequestResultMailViewModel>(),
                        _ => mailInfo.MailObject.ToObject<object>()
                    };
                    Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
                    var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
                    await _mailOperationService.SendPodcastServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
                    // SagaEventMessage KafkaProducerService.PrepareSagaEventMessage(string topic, JObject requestData, JObject responseData, Guid? sagaInstanceId, string flowName, string messageName, [string? key = null])
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: SAGA_TOPIC,
                        requestData: command.RequestData,
                        responseData: command.RequestData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "send-user-service-email.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "send-user-service-email.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-channel", SAGA_TOPIC)]
        public async Task HandleCreateChannelAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var createChannelParameterDTO = command.RequestData.ToObject<CreateChannelParameterDTO>();
                    await _podcastChannelService.CreatePodcastChannel(createChannelParameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-channel.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-channel", SAGA_TOPIC)]
        public async Task HandleUpdateChannelAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var updateChannelParameterDTO = command.RequestData.ToObject<UpdateChannelParameterDTO>();
                    await _podcastChannelService.UpdatePodcastChannel(updateChannelParameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-channel.failed"    // From YAML onFailure.emit
            );

        }

        // [MessageHandler("delete-channel", SAGA_TOPIC)] 

        [MessageHandler("publish-channel", SAGA_TOPIC)]
        public async Task HandlePublishChannelAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var channelId = command.RequestData.ToObject<PublishChannelParameterDTO>();
                    await _podcastChannelService.PublishPodcastChannel(channelId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "publish-channel.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("plus-channel-total-favorite", SAGA_TOPIC)]
        public async Task HandlePlusChannelTotalFavoriteAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    // throw new Exception("Simulated exception for testing failure handling.");
                    var channelId = command.RequestData.ToObject<PlusChannelTotalFavoriteParameterDTO>();
                    await _podcastChannelService.PlusPodcastChannelTotalFavorite(channelId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "plus-channel-total-favorite.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("subtract-channel-total-favorite", SAGA_TOPIC)]
        public async Task HandleSubtractChannelTotalFavoriteAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    // throw new Exception("Simulated exception for testing failure handling.");
                    var channelId = command.RequestData.ToObject<SubtractChannelTotalFavoriteParameterDTO>();
                    await _podcastChannelService.SubtractPodcastChannelTotalFavorite(channelId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "subtract-channel-total-favorite.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-show", SAGA_TOPIC)]
        public async Task HandleShowCreationFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var show = command.RequestData.ToObject<CreateShowParameterDTO>();
                    await _podcastShowService.CreatePodcastShow(show, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-show.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("assign-show-channel", SAGA_TOPIC)]
        public async Task HandleShowChannelAssignFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var show = command.RequestData.ToObject<AssignShowChannelParameterDTO>();
                    await _podcastShowService.AssignPodcastShowChannel(show, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "assign-show-channel.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-show", SAGA_TOPIC)]
        public async Task HandleShowUpdateFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var show = command.RequestData.ToObject<UpdateShowParameterDTO>();
                    await _podcastShowService.UpdatePodcastShow(show, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-show.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("submit-show-trailer-audio-file", SAGA_TOPIC)]
        public async Task HandleShowTrailerAudioSubmissionFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var show = command.RequestData.ToObject<SubmitShowTrailerAudioFileParameterDTO>();
                    await _podcastShowService.SubmitPodcastShowTrailerAudioFile(show, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "submit-show-trailer-audio-file.failed"    // From YAML onFailure.emit
            );
        }


        [MessageHandler("publish-show", SAGA_TOPIC)]
        public async Task HandleShowPublishingFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var show = command.RequestData.ToObject<PublishShowParameterDTO>();
                    await _podcastShowService.PublishPodcastShow(show, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "publish-show.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("plus-show-total-follow", SAGA_TOPIC)]
        public async Task HandlePlusShowTotalFollowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var showId = command.RequestData.ToObject<PlusShowTotalFollowParameterDTO>();
                    await _podcastShowService.PlusPodcastShowTotalFollow(showId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "plus-show-total-follow.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("subtract-show-total-follow", SAGA_TOPIC)]
        public async Task HandleSubtractShowTotalFollowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var showId = command.RequestData.ToObject<SubtractShowTotalFollowParameterDTO>();
                    await _podcastShowService.SubtractPodcastShowTotalFollow(showId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "subtract-show-total-follow.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("plus-episode-total-saved", SAGA_TOPIC)]
        public async Task HandlePlusEpisodeTotalSavedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episodeId = command.RequestData.ToObject<PlusEpisodeTotalSavedParameterDTO>();
                    await _podcastEpisodeService.PlusPodcastEpisodeTotalSaved(episodeId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "plus-episode-total-saved.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("subtract-episode-total-saved", SAGA_TOPIC)]
        public async Task HandleSubtractEpisodeTotalSavedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episodeId = command.RequestData.ToObject<SubtractEpisodeTotalSavedParameterDTO>();
                    await _podcastEpisodeService.SubtractPodcastEpisodeTotalSaved(episodeId, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "subtract-episode-total-saved.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-episode", SAGA_TOPIC)]
        public async Task HandleCreateEpisodeAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<CreateEpisodeParameterDTO>();
                    await _podcastEpisodeService.CreatePodcastEpisode(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-episode.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-episode", SAGA_TOPIC)]
        public async Task HandleUpdateEpisodeAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<UpdateEpisodeParameterDTO>();
                    await _podcastEpisodeService.UpdatePodcastEpisode(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-episode.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("upload-episode-licenses", SAGA_TOPIC)]
        public async Task HandleUploadEpisodeLicenseFilesAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var uploadEpisodeLicenses = command.RequestData.ToObject<UploadEpisodeLicensesParameterDTO>();
                    await _podcastEpisodeService.UploadPodcastEpisodeLicenseFiles(uploadEpisodeLicenses, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "upload-episode-licenses.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-episode-licenses", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeLicensesAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var deleteEpisodeLicenses = command.RequestData.ToObject<DeleteEpisodeLicensesParameterDTO>();
                    await _podcastEpisodeService.DeletePodcastEpisodeLicenseFiles(deleteEpisodeLicenses, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-episode-licenses.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("submit-episode-audio-file", SAGA_TOPIC)]
        public async Task HandleEpisodeAudioSubmissionFlowAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<SubmitEpisodeAudioFileParameterDTO>();
                    await _podcastEpisodeService.SubmitPodcastEpisodeAudioFile(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "submit-episode-audio-file.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("processing-episode-draft-audio", SAGA_TOPIC)]
        public async Task HandleProcessingEpisodeDraftAudioAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<ProcessingEpisodeDraftAudioParameterDTO>();
                    await _podcastEpisodeService.ProcessPodcastEpisodeDraftAudio(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "processing-episode-draft-audio.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("publish-episode", SAGA_TOPIC)]
        public async Task HandlePublishEpisodeAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<PublishEpisodeParameterDTO>();
                    await _podcastEpisodeService.PublishPodcastEpisode(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "publish-episode.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("processing-episode-publish-audio", SAGA_TOPIC)]
        public async Task HandleProcessingEpisodePublishAudioAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var episode = command.RequestData.ToObject<ProcessingEpisodePublishAudioParameterDTO>();
                    await _podcastEpisodeService.ProcessPodcastEpisodePublishAudio(episode, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "processing-episode-publish-audio.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("create-background-sound-track", SAGA_TOPIC)]
        public async Task HandleCreateBackgroundSoundTrackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var backgroundSoundTrack = command.RequestData.ToObject<CreateBackgroundSoundTrackParameterDTO>();
                    await _podcastBackgroundSoundTrackService.CreateBackgroundSoundTrack(backgroundSoundTrack, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "create-background-sound-track.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-background-sound-track", SAGA_TOPIC)]
        public async Task HandleUpdateBackgroundSoundTrackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var backgroundSoundTrack = command.RequestData.ToObject<UpdateBackgroundSoundTrackParameterDTO>();
                    await _podcastBackgroundSoundTrackService.UpdateBackgroundSoundTrack(backgroundSoundTrack, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-background-sound-track.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-background-sound-track", SAGA_TOPIC)]
        public async Task HandleDeleteBackgroundSoundTrackAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var backgroundSoundTrack = command.RequestData.ToObject<DeleteBackgroundSoundTrackParameterDTO>();
                    await _podcastBackgroundSoundTrackService.DeleteBackgroundSoundTrack(backgroundSoundTrack, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-background-sound-track.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-episode-listen-session-content-dmca-remove-episode-force", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeListenSessionDmcaRemoveEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveEpisodeListenSessionContentDmcaRemoveEpisodeForceParameterDTO>();
                    await _podcastEpisodeService.RemoveEpisodeListenSessionContentDmcaRemoveEpisodeForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-episode-listen-session-content-dmca-remove-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-episode-dmca-remove-episode-force", SAGA_TOPIC)]
        public async Task HandleRemoveEpisodeDmcaRemoveEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveEpisodeDmcaRemoveEpisodeForceParameterDTO>();
                    await _podcastEpisodeService.RemoveEpisodeDmcaRemoveEpisodeForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-episode-dmca-remove-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-episode-dmca-unpublish-episode-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedEpisodeDmcaUnpublishEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedEpisodeDmcaUnpublishEpisodeForceParameterDTO>();
                    await _podcastEpisodeService.RemoveDismissedEpisodeDmcaUnpublishEpisodeForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-episode-dmca-unpublish-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-episode-listen-session-content-unpublish-episode-force", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeListenSessionUnpublishEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveEpisodeListenSessionContentUnpublishEpisodeForceParameterDTO>();
                    await _podcastEpisodeService.RemoveEpisodeListenSessionContentUnpublishEpisodeForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-episode-listen-session-content-unpublish-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("unpublish-episode-unpublish-episode-force", SAGA_TOPIC)]
        public async Task HandleUnpublishEpisodeUnpublishEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<UnpublishEpisodeUnpublishEpisodeForceParameterDTO>();
                    await _podcastEpisodeService.UnpublishEpisodeUnpublishEpisodeForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "unpublish-episode-unpublish-episode-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-show-episodes-listen-session-content-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowEpisodesListenSessionDmcaRemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveShowEpisodesListenSessionContentDmcaRemoveShowForceParameterDTO>();
                    // await _podcastEpisodeService.DeleteShowEpisodesListenSessionDmcaRemoveShowForce(parameterDTO, command);
                    await _podcastEpisodeService.RemoveShowEpisodesListenSessionContentDmcaRemoveShowForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-show-episodes-listen-session-content-dmca-remove-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-show-episodes-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleRemoveShowEpisodesDmcaRemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveShowEpisodesDmcaRemoveShowForceParameterDTO>();
                    await _podcastEpisodeService.RemoveShowEpisodesDmcaRemoveShowForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-show-episodes-dmca-remove-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-show-dmca-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleRemoveShowDmcaDmcaRemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveShowDmcaDmcaRemoveShowForceParameterDTO>();
                    await _podcastShowService.RemoveShowDmcaRemoveShowForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-show-dmca-dmca-remove-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-show-dmca-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedShowDmcaUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedShowDmcaUnpublishShowForceParameterDTO>();
                    await _podcastShowService.RemoveDismissedShowDmcaUnpublishShowForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-show-dmca-unpublish-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-show-episodes-dmca-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedShowEpisodesDmcaUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO>();
                    await _podcastEpisodeService.RemoveDismissedShowEpisodesDmcaUnpublishShowForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-show-episodes-dmca-unpublish-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-show-episodes-listen-session-content-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowEpisodesListenSessionUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveShowEpisodesListenSessionContentUnpublishShowForceParameterDTO>();
                    await _podcastEpisodeService.RemoveShowEpisodesListenSessionContentUnpublishShowForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-show-episodes-listen-session-content-unpublish-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("unpublish-show-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleUnpublishShowUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<UnpublishShowUnpublishShowForceParameterDTO>();
                    await _podcastShowService.UnpublishShowUnpublishShowForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "unpublish-show-unpublish-show-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-channel-dismissed-shows-dmca-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleRemoveChannelDismissedShowsDmcaUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO>();
                    await _podcastShowService.RemoveChannelDismissedShowsDmcaUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-channel-dismissed-shows-dmca-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-channel-dismissed-episodes-dmca-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleRemoveChannelDismissedEpisodesDmcaUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO>();
                    await _podcastEpisodeService.RemoveChannelDismissedEpisodesDmcaUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-channel-dismissed-episodes-dmca-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-channel-episodes-listen-session-content-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelEpisodesListenSessionUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO>();
                    await _podcastEpisodeService.RemoveChannelEpisodesListenSessionContentUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-channel-episodes-listen-session-content-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("unpublish-channel-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleUnpublishChannelUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<UnpublishChannelUnpublishChannelForceParameterDTO>();
                    await _podcastChannelService.UnpublishChannelUnpublishChannelForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "unpublish-channel-unpublish-channel-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-episode-listen-session-content-episode-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeListenSessionEpisodeDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveEpisodeListenSessionContentEpisodeDeletionForceParameterDTO>();
                    await _podcastEpisodeService.RemoveEpisodeListenSessionContentEpisodeDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-episode-listen-session-content-episode-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-episode-dmca-episode-deletion-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedEpisodeDmcaEpisodeDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO>();
                    await _podcastEpisodeService.RemoveDismissedEpisodeDmcaEpisodeDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-episode-dmca-episode-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-episode-episode-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteEpisodeEpisodeDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteEpisodeEpisodeDeletionForceParameterDTO>();
                    await _podcastEpisodeService.DeleteEpisodeEpisodeDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-episode-episode-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-show-dmca-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedShowDmcaShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    // var parameterDTO = command.RequestData.ToObject<DismissShowDmcaShowDeletionForceParameterDTO>();
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedShowDmcaShowDeletionForceParameterDTO>();
                    await _podcastShowService.RemoveDismissedShowDmcaShowDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-show-dmca-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-show-episodes-listen-session-content-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowEpisodesListenSessionShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveShowEpisodesListenSessionContentShowDeletionForceParameterDTO>();
                    // await _podcastEpisodeService.DeleteShowEpisodesListenSessionShowDeletionForce(parameterDTO, command);
                    await _podcastEpisodeService.RemoveShowEpisodesListenSessionContentShowDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-show-episodes-listen-session-content-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-show-episodes-dmca-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedShowEpisodesDmcaShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedShowEpisodesDmcaShowDeletionForceParameterDTO>();
                    await _podcastEpisodeService.RemoveDismissedShowEpisodesDmcaShowDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-show-episodes-dmca-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-episodes-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowEpisodesShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteShowEpisodesShowDeletionForceParameterDTO>();
                    await _podcastEpisodeService.DeleteShowEpisodesShowDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-episodes-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-show-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteShowShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteShowShowShowDeletionForceParameterDTO>();
                    await _podcastShowService.DeleteShowShowShowDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-show-show-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("keep-channel-shows-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleKeepChannelShowsChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<KeepChannelShowsChannelDeletionForceParameterDTO>();
                    await _podcastShowService.KeepChannelShowsChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "keep-channel-shows-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-channel-dismissed-shows-dmca-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleRemoveChannelDismissedShowsDmcaChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveChannelDismissedShowsDmcaChannelDeletionForceParameterDTO>();
                    await _podcastShowService.RemoveChannelDismissedShowsDmcaChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-channel-dismissed-shows-dmca-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-channel-episodes-listen-session-content-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelEpisodesListenSessionChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveChannelEpisodesListenSessionContentChannelDeletionForceParameterDTO>();
                    // await _podcastEpisodeService.DeleteChannelEpisodesListenSessionChannelDeletionForce(parameterDTO, command);
                    await _podcastEpisodeService.RemoveChannelEpisodesListenSessionContentChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-channel-episodes-listen-session-content-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-channel-dismissed-episodes-dmca-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleRemoveChannelDismissedEpisodesDmcaChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO>();
                    await _podcastEpisodeService.RemoveChannelDismissedEpisodesDmcaChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-channel-dismissed-episodes-dmca-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-channel-episodes-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelEpisodesChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteChannelEpisodesChannelDeletionForceParameterDTO>();
                    await _podcastEpisodeService.DeleteChannelEpisodesChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-episodes-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-channel-shows-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelShowsChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteChannelShowsChannelDeletionForceParameterDTO>();
                    await _podcastShowService.DeleteChannelShowsChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-shows-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("delete-channel-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDeleteChannelChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<DeleteChannelChannelDeletionForceParameterDTO>();
                    await _podcastChannelService.DeleteChannelChannelDeletionForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "delete-channel-channel-deletion-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("unpublish-podcaster-channels-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleUnpublishPodcasterChannelsTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<UnpublishPodcasterChannelsTerminatePodcasterForceParameterDTO>();
                    await _podcastChannelService.UnpublishPodcasterChannelsTerminatePodcasterForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "unpublish-podcaster-channels-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("unpublish-podcaster-shows-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleUnpublishPodcasterShowsTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<UnpublishPodcasterShowsTerminatePodcasterForceParameterDTO>();
                    await _podcastShowService.UnpublishPodcasterShowsTerminatePodcasterForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "unpublish-podcaster-shows-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-show-dmca-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedShowDmcaTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedShowDmcaTerminatePodcasterForceParameterDTO>();
                    await _podcastShowService.RemoveDismissedShowDmcaTerminatePodcasterForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-show-dmca-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("unpublish-podcaster-episodes-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleUnpublishPodcasterEpisodesTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<UnpublishPodcasterEpisodesTerminatePodcasterForceParameterDTO>();
                    await _podcastEpisodeService.UnpublishPodcasterEpisodesTerminatePodcasterForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "unpublish-podcaster-episodes-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-dismissed-episode-dmca-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleRemoveDismissedEpisodeDmcaTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemoveDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO>();
                    await _podcastEpisodeService.RemoveDismissedEpisodeDmcaTerminatePodcasterForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-dismissed-episode-dmca-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("remove-podcaster-episodes-listen-session-content-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDeletePodcasterEpisodesListenSessionTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RemovePodcasterEpisodesListenSessionContentTerminatePodcasterForceParameterDTO>();
                    await _podcastEpisodeService.RemovePodcasterEpisodesListenSessionContentTerminatePodcasterForce(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "remove-podcaster-episodes-listen-session-content-terminate-podcaster-force.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("update-episode-listen-session-duration", SAGA_TOPIC)]
        public async Task HandleUpdateEpisodeListenSessionDurationAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<UpdateEpisodeListenSessionDurationParameterDTO>();
                    await _podcastEpisodeService.UpdateEpisodeListenSessionDuration(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "update-episode-listen-session-duration.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("takedown-content-dmca", SAGA_TOPIC)]
        public async Task HandleTakedownContentAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<TakedownContentDmcaParameterDTO>();
                    if (parameterDTO.PodcastEpisodeId == null && parameterDTO.PodcastShowId == null)
                    {
                        throw new ArgumentNullException(nameof(parameterDTO), "Parameter DTO cannot be null");
                    }
                    else if (parameterDTO.PodcastEpisodeId != null && parameterDTO.PodcastShowId != null)
                    {
                        throw new ArgumentException("Both PodcastEpisodeId and PodcastShowId cannot be set at the same time");
                    }
                    else if (parameterDTO.PodcastEpisodeId != null)
                    {
                        await _podcastEpisodeService.TakedownContentDmcaEpisode(parameterDTO, command);
                    }
                    else if (parameterDTO.PodcastShowId != null)
                    {
                        await _podcastShowService.TakedownContentDmcaShow(parameterDTO, command);
                    }
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "takedown-content-dmca.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("restore-content-dmca", SAGA_TOPIC)]
        public async Task HandleRestoreContentAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<RestoreContentDmcaParameterDTO>();
                    if (parameterDTO.PodcastEpisodeId == null || parameterDTO.PodcastShowId == null)
                    {
                        throw new ArgumentNullException(nameof(parameterDTO), "Parameter DTO cannot be null");
                    }
                    else if (parameterDTO.PodcastEpisodeId != null && parameterDTO.PodcastShowId != null)
                    {
                        throw new ArgumentException("Both PodcastEpisodeId and PodcastShowId cannot be set at the same time");
                    }
                    else if (parameterDTO.PodcastEpisodeId != null)
                    {
                        await _podcastEpisodeService.RestoreContentDmcaEpisode(parameterDTO, command);
                    }
                    else if (parameterDTO.PodcastShowId != null)
                    {
                        await _podcastShowService.RestoreContentDmcaShow(parameterDTO, command);
                    }
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "restore-content-dmca.failed"    // From YAML onFailure.emit
            );
        }

        [MessageHandler("complete-all-user-episode-listen-sessions", SAGA_TOPIC)]
        public async Task HandleCompleteAllUserEpisodeListenSessionsAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
                messageJson: messageJson,
                stepHandler: async (command) =>
                {
                    var parameterDTO = command.RequestData.ToObject<CompleteAllUserEpisodeListenSessionsParameterDTO>();
                    await _podcastEpisodeService.CompleteAllUserEpisodeListenSessions(parameterDTO, command);
                },
                responseTopic: SAGA_TOPIC,
                failedEmitMessage: "complete-all-user-episode-listen-sessions.failed"    // From YAML onFailure.emit
            );
        }
    }
}

