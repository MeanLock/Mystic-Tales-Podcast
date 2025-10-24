using Microsoft.Extensions.Logging;
using PodcastService.BusinessLogic.Attributes;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusChannelTotalFavorite;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusShowTotalFollow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodeDraftAudio;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitEpisodeAudioFile;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitShowTrailerAudioFile;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractChannelTotalFavorite;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractShowTotalFollow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UploadEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.AcceptEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RejectEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequestEpisodeAudioExamination;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequireEpisodePublishReviewSessionEdit;
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

        // [MessageHandler("send-user-service-email", SAGA_TOPIC)]
        // public async Task HandleSendPodcastServiceEmailAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var sendPodcastServiceEmailParameterDTO = command.RequestData.ToObject<SendPodcastServiceEmailParameterDTO>();
        //             var mailInfo = sendPodcastServiceEmailParameterDTO.SendPodcastServiceEmailMailInfo;
        //             Console.WriteLine("Preparing to send email of type: " + mailInfo.MailTypeName);
        //             object mailModel = mailInfo.MailTypeName switch
        //             {
        //                 "CustomerRegistrationVerification" => mailInfo.MailObject.ToObject<CustomerRegistrationVerificationMailViewModel>(),
        //                 "CustomerPasswordReset" => mailInfo.MailObject.ToObject<CustomerPasswordResetMailViewModel>(),
        //                 "PodcasterRequestConfirmation" => mailInfo.MailObject.ToObject<PodcasterRequestConfirmationMailViewModel>(),
        //                 "PodcasterRequestResult" => mailInfo.MailObject.ToObject<PodcasterRequestResultMailViewModel>(),
        //                 _ => mailInfo.MailObject.ToObject<object>()
        //             };
        //             Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
        //             var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
        //             await _mailOperationService.SendPodcastServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
        //             // SagaEventMessage KafkaProducerService.PrepareSagaEventMessage(string topic, JObject requestData, JObject responseData, Guid? sagaInstanceId, string flowName, string messageName, [string? key = null])
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: SAGA_TOPIC,
        //                 requestData: command.RequestData,
        //                 responseData: command.RequestData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "send-user-service-email.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "send-user-service-email.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("create-channel", SAGA_TOPIC)]
        // public async Task HandleCreateChannelAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var createChannelParameterDTO = command.RequestData.ToObject<CreateChannelParameterDTO>();
        //             await _podcastChannelService.CreatePodcastChannel(createChannelParameterDTO, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "create-channel.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("update-channel", SAGA_TOPIC)]
        // public async Task HandleUpdateChannelAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var updateChannelParameterDTO = command.RequestData.ToObject<UpdateChannelParameterDTO>();
        //             await _podcastChannelService.UpdatePodcastChannel(updateChannelParameterDTO, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "update-channel.failed"    // From YAML onFailure.emit
        //     );

        // }

        // // [MessageHandler("delete-channel", SAGA_TOPIC)] 

        // [MessageHandler("publish-channel", SAGA_TOPIC)]
        // public async Task HandlePublishChannelAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var channelId = command.RequestData.ToObject<PublishChannelParameterDTO>();
        //             await _podcastChannelService.PublishPodcastChannel(channelId, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "publish-channel.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("plus-channel-total-favorite", SAGA_TOPIC)]
        // public async Task HandlePlusChannelTotalFavoriteAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             // throw new Exception("Simulated exception for testing failure handling.");
        //             var channelId = command.RequestData.ToObject<PlusChannelTotalFavoriteParameterDTO>();
        //             await _podcastChannelService.PlusPodcastChannelTotalFavorite(channelId, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "plus-channel-total-favorite.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("subtract-channel-total-favorite", SAGA_TOPIC)]
        // public async Task HandleSubtractChannelTotalFavoriteAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             // throw new Exception("Simulated exception for testing failure handling.");
        //             var channelId = command.RequestData.ToObject<SubtractChannelTotalFavoriteParameterDTO>();
        //             await _podcastChannelService.SubtractPodcastChannelTotalFavorite(channelId, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "subtract-channel-total-favorite.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("create-show", SAGA_TOPIC)]
        // public async Task HandleShowCreationFlowAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var show = command.RequestData.ToObject<CreateShowParameterDTO>();
        //             await _podcastShowService.CreatePodcastShow(show, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "create-show.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("update-show", SAGA_TOPIC)]
        // public async Task HandleShowUpdateFlowAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var show = command.RequestData.ToObject<UpdateShowParameterDTO>();
        //             await _podcastShowService.UpdatePodcastShow(show, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "update-show.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("submit-show-trailer-audio-file", SAGA_TOPIC)]
        // public async Task HandleShowTrailerAudioSubmissionFlowAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var show = command.RequestData.ToObject<SubmitShowTrailerAudioFileParameterDTO>();
        //             await _podcastShowService.SubmitPodcastShowTrailerAudioFile(show, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "submit-show-trailer-audio-file.failed"    // From YAML onFailure.emit
        //     );
        // }


        // [MessageHandler("publish-show", SAGA_TOPIC)]
        // public async Task HandleShowPublishingFlowAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var show = command.RequestData.ToObject<PublishShowParameterDTO>();
        //             await _podcastShowService.PublishPodcastShow(show, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "publish-show.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("plus-show-total-follow", SAGA_TOPIC)]
        // public async Task HandlePlusShowTotalFollowAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var showId = command.RequestData.ToObject<PlusShowTotalFollowParameterDTO>();
        //             await _podcastShowService.PlusPodcastShowTotalFollow(showId, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "plus-show-total-follow.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("subtract-show-total-follow", SAGA_TOPIC)]
        // public async Task HandleSubtractShowTotalFollowAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var showId = command.RequestData.ToObject<SubtractShowTotalFollowParameterDTO>();
        //             await _podcastShowService.SubtractPodcastShowTotalFollow(showId, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "subtract-show-total-follow.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("plus-episode-total-saved", SAGA_TOPIC)]
        // public async Task HandlePlusEpisodeTotalSavedAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var episodeId = command.RequestData.ToObject<PlusEpisodeTotalSavedParameterDTO>();
        //             await _podcastEpisodeService.PlusPodcastEpisodeTotalSaved(episodeId, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "plus-episode-total-saved.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("subtract-episode-total-saved", SAGA_TOPIC)]
        // public async Task HandleSubtractEpisodeTotalSavedAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var episodeId = command.RequestData.ToObject<SubtractEpisodeTotalSavedParameterDTO>();
        //             await _podcastEpisodeService.SubtractPodcastEpisodeTotalSaved(episodeId, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "subtract-episode-total-saved.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("create-episode", SAGA_TOPIC)]
        // public async Task HandleCreateEpisodeAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var episode = command.RequestData.ToObject<CreateEpisodeParameterDTO>();
        //             await _podcastEpisodeService.CreatePodcastEpisode(episode, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "create-episode.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("update-episode", SAGA_TOPIC)]
        // public async Task HandleUpdateEpisodeAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var episode = command.RequestData.ToObject<UpdateEpisodeParameterDTO>();
        //             await _podcastEpisodeService.UpdatePodcastEpisode(episode, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "update-episode.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("upload-episode-licenses", SAGA_TOPIC)]
        // public async Task HandleUploadEpisodeLicenseFilesAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var uploadEpisodeLicenses = command.RequestData.ToObject<UploadEpisodeLicensesParameterDTO>();
        //             await _podcastEpisodeService.UploadPodcastEpisodeLicenseFiles(uploadEpisodeLicenses, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "upload-episode-licenses.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("delete-episode-licenses", SAGA_TOPIC)]
        // public async Task HandleDeleteEpisodeLicensesAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var deleteEpisodeLicenses = command.RequestData.ToObject<DeleteEpisodeLicensesParameterDTO>();
        //             await _podcastEpisodeService.DeletePodcastEpisodeLicenseFiles(deleteEpisodeLicenses, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "delete-episode-licenses.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("submit-episode-audio-file", SAGA_TOPIC)]
        // public async Task HandleEpisodeAudioSubmissionFlowAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var episode = command.RequestData.ToObject<SubmitEpisodeAudioFileParameterDTO>();
        //             await _podcastEpisodeService.SubmitPodcastEpisodeAudioFile(episode, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "submit-episode-audio-file.failed"    // From YAML onFailure.emit
        //     );
        // }

        // [MessageHandler("processing-episode-draft-audio", SAGA_TOPIC)]
        // public async Task HandlePublishEpisodeAsync(string key, string messageJson)
        // {
        //     await ExecuteSagaCommandMessageAsync(
        //         messageJson: messageJson,
        //         stepHandler: async (command) =>
        //         {
        //             var episode = command.RequestData.ToObject<ProcessingEpisodeDraftAudioParameterDTO>();
        //             await _podcastEpisodeService.ProcessPodcastEpisodeDraftAudio(episode, command);
        //         },
        //         responseTopic: SAGA_TOPIC,
        //         failedEmitMessage: "publish-episode.failed"    // From YAML onFailure.emit
        //     );
        // }
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

    }
}

