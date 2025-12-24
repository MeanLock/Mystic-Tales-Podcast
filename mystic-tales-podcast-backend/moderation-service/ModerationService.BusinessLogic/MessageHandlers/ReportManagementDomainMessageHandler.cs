using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.Attributes;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateEpisodeReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreatePodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateShowReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastShowReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.SendModerationServiceEmail;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReportNoEffectDMCARemoveEpisodeForce;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Models.Mail;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Common.AppConfigurations.BusinessSetting;
using ModerationService.Infrastructure.Services.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowReportNoEffectDMCARemoveShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowEpisodesReportNoEffectDMCARemoveShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowReportNoEffectUnpublishShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowEpisodesReportNoEffectUnpublishShowForce;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelEpisodesReportNoEffectUnpublishChannelForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelShowsReportNoEffectUnpublishChannelForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReportNoEffectEpisodeDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowReportNoEffectShowDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowEpisodesReportNoEffectShowDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelShowsReportNoEffectChannelDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelEpisodesReportNoEffectChannelDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastBuddyReportNoEffectTerminatePodcasterForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcasterShowsReportNoEffectTerminatePodcasterForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForce;

namespace ModerationService.BusinessLogic.MessageHandlers
{
    public class ReportManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly ILogger<ReportManagementDomainMessageHandler> _logger;
        private readonly PodcastBuddyReportService _podcastBuddyReportService;
        private readonly PodcastShowReportService _podcastShowReportService;
        private readonly PodcastEpisodeReportService _podcastEpisodeReportService;
        private readonly IMailPropertiesConfig _mailPropertiesConfig;
        private readonly MailOperationService _mailOperationService;
        private readonly KafkaProducerService _kafkaProducerService;
        private const string SAGA_TOPIC = KafkaTopicEnum.ReportManagementDomain;
        public ReportManagementDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<ReportManagementDomainMessageHandler> logger,
            PodcastBuddyReportService podcastBuddyReportService,
            PodcastShowReportService podcastShowReportService,
            PodcastEpisodeReportService podcastEpisodeReportService,
            IMailPropertiesConfig mailPropertiesConfig,
            MailOperationService mailOperationService
            ) : base(messagingService, kafkaProducerService, logger)
        {
            _logger = logger;
            _podcastBuddyReportService = podcastBuddyReportService;
            _podcastShowReportService = podcastShowReportService;
            _podcastEpisodeReportService = podcastEpisodeReportService;
            _mailPropertiesConfig = mailPropertiesConfig;
            _mailOperationService = mailOperationService;
            _kafkaProducerService = kafkaProducerService;
        }
        [MessageHandler("create-podcast-buddy-report", SAGA_TOPIC)]
        public async Task HandlerCreatePodcastBuddyReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CreatePodcastBuddyReportParameterDTO>();
                   await _podcastBuddyReportService.CreatePodcastBuddyReportAsync(parameter, command);
                   _logger.LogInformation("Handled create-podcast-buddy-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "create-podcast-buddy-report.failed"
           );
        }
        [MessageHandler("resolve-podcast-buddy-report", SAGA_TOPIC)]
        public async Task HandleResolvePodcastBuddyReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolvePodcastBuddyReportParameterDTO>();
                   await _podcastBuddyReportService.ResolvePodcastBuddyReportReviewSessionAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-podcast-buddy-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-podcast-buddy-report.failed"
           );
        }
        [MessageHandler("create-show-report", SAGA_TOPIC)]
        public async Task HandlerCreatePodcastShowReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CreateShowReportParameterDTO>();
                   await _podcastShowReportService.CreatePodcastShowReportAsync(parameter, command);
                   _logger.LogInformation("Handled create-show-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "create-show-report.failed"
           );
        }
        [MessageHandler("resolve-show-report", SAGA_TOPIC)]
        public async Task HandlerResolvePodcastShowReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveShowReportParameterDTO>();
                   await _podcastShowReportService.ResolveShowReportReviewSessionAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-show-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-show-report.failed"
           );
        }
        [MessageHandler("create-episode-report", SAGA_TOPIC)]
        public async Task HandlerCreatePodcastEpisodeReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CreateEpisodeReportParameterDTO>();
                   await _podcastEpisodeReportService.CreatePodcastEpisodeReportAsync(parameter, command);
                   _logger.LogInformation("Handled create-episode-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "create-episode-report.failed"
           );
        }
        [MessageHandler("resolve-episode-report", SAGA_TOPIC)]
        public async Task HandlerResolvePodcastEpisodeReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveEpisodeReportParameterDTO>();
                   await _podcastEpisodeReportService.ResolveEpisodeReportReviewSessionAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-episode-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-episode-report.failed"
           );
        }
        [MessageHandler("send-moderation-service-email", SAGA_TOPIC)]
        public async Task HandleSendModerationServiceEmailAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var sendModerationServiceEmailParameterDTO = command.RequestData.ToObject<SendModerationServiceEmailParameterDTO>();
                   var mailInfo = sendModerationServiceEmailParameterDTO.SendModerationServiceEmailInfo;
                   Console.WriteLine("Preparing to send email of type: " + mailInfo.MailTypeName);
                   object mailModel = mailInfo.MailTypeName switch
                   {
                       "DMCANoticePending" => mailInfo.MailObject.ToObject<DMCANoticePendingMailViewModel>(),
                       "DMCACounterNoticePending" => mailInfo.MailObject.ToObject<DMCACounterNoticePendingMailViewModel>(),
                       "DMCALawsuitProofPending" => mailInfo.MailObject.ToObject<DMCALawsuitProofPendingMailViewModel>(),
                       "DMCANoticeInvalid" => mailInfo.MailObject.ToObject<DMCANoticeInvalidMailViewModel>(),
                       "DMCACounterNoticeInvalidToAccused" => mailInfo.MailObject.ToObject<DMCACounterNoticeInvalidToAccusedMailViewModel>(),
                       "DMCACounterNoticeInvalidToAccuser" => mailInfo.MailObject.ToObject<DMCACounterNoticeInvalidToAccuserMailViewModel>(),
                       "DMCALawsuitProofInvalidToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofInvalidToAccusedMailViewModel>(),
                       "DMCALawsuitProofInvalidToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofInvalidToAccuserMailViewModel>(),
                       "DMCALawsuitProofPodcasterWinToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofPodcasterWinToAccusedMailViewModel>(),
                       "DMCALawsuitProofPodcasterWinToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofPodcasterWinToAccuserMailViewModel>(),
                       "DMCALawsuitProofAccuserWinToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofAccuserWinToAccusedMailViewModel>(),
                       "DMCALawsuitProofAccuserWinToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofAccuserWinToAccuserMailViewModel>(),
                       "DMCANoticeValidToAccuser" => mailInfo.MailObject.ToObject<DMCANoticeValidToAccuserMailViewModel>(),
                       "DMCANoticeValidToAccused" => mailInfo.MailObject.ToObject<DMCANoticeValidToAccusedMailViewModel>(),
                       "DMCANoticeValidNotResponseInTimeToAccused" => mailInfo.MailObject.ToObject<DMCANoticeValidNotResponseInTimeToAccusedMailViewModel>(),
                       "DMCANoticeValidNotResponseInTimeToAccuser" => mailInfo.MailObject.ToObject<DMCANoticeValidNotResponseInTimeToAccuserMailViewModel>(),
                       "DMCANoticeValidAgreeTakenDownToAccused" => mailInfo.MailObject.ToObject<DMCANoticeValidAgreeTakenDownToAccusedMailViewModel>(),
                       "DMCANoticeValidAgreeTakenDownToAccuser" => mailInfo.MailObject.ToObject<DMCANoticeValidAgreeTakenDownToAccuserMailViewModel>(),
                       "DMCACounterNoticeConfirmation" => mailInfo.MailObject.ToObject<DMCACounterNoticeConfirmationMailViewModel>(),
                       "DMCACounterNoticeValidToAccused" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidToAccusedMailViewModel>(),
                       "DMCACounterNoticeValidToAccuser" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidToAccuserMailViewModel>(),
                       "DMCACounterNoticeValidNotResponseInTimeToAccused" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidNotResponseInTimeToAccusedMailViewModel>(),
                       "DMCACounterNoticeValidNotResponseInTimeToAccuser" => mailInfo.MailObject.ToObject<DMCACounterNoticeValidNotResponseInTimeToAccuserMailViewModel>(),
                       "DMCALawsuitProofValidToAccused" => mailInfo.MailObject.ToObject<DMCALawsuitProofValidToAccusedMailViewModel>(),
                       "DMCALawsuitProofValidToAccuser" => mailInfo.MailObject.ToObject<DMCALawsuitProofValidToAccuserMailViewModel>(),
                       _ => mailInfo.MailObject.ToObject<object>()
                   };
                   //Console.WriteLine("Sending email to: " + mailInfo.MailObject["VerifyCode"]);
                   var mailProperty = _mailPropertiesConfig.GetMailPropertyByTypeName(mailInfo.MailTypeName);
                   await _mailOperationService.SendModerationServiceEmail(mailProperty, mailInfo.ToEmail, mailModel);
                   // SagaEventMessage KafkaProducerService.PrepareSagaEventMessage(string topic, JObject requestData, JObject responseData, Guid? sagaInstanceId, string flowName, string messageName, [string? key = null])
                   var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                       topic: SAGA_TOPIC,
                       requestData: command.RequestData,
                       responseData: command.RequestData,
                       sagaInstanceId: command.SagaInstanceId,
                       flowName: command.FlowName,
                       messageName: "send-moderation-service-email.success"
                   );
                   await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                   _logger.LogInformation("Handled send-moderation-service-email command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "send-moderation-service-email.failed"
           );
        }
        [MessageHandler("resolve-episode-report-no-effect-dmca-remove-episode-force", SAGA_TOPIC)]
        public async Task HandleResolveEpisodeReportNoEffectDMCARemoveEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveEpisodeReportNoEffectDMCARemoveEpisodeForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolveEpisodeReportNoEffectDMCARemoveEpisodeForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-episode-report-no-effect-dmca-remove-episode-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-episode-report-no-effect-dmca-remove-episode-force.failed"
           );
        }
        [MessageHandler("resolve-show-report-no-effect-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleResolveShowReportNoEffectDMCARemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveShowReportNoEffectDMCARemoveShowForceParameterDTO>();
                   await _podcastShowReportService.ResolveShowReportNoEffectDMCARemoveShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-show-report-no-effect-dmca-remove-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-show-report-no-effect-dmca-remove-show-force.failed"
           );
        }
        [MessageHandler("resolve-show-episodes-report-no-effect-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleResolveShowEpisodesReportNoEffectDMCARemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveShowEpisodesReportNoEffectDMCARemoveShowForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolveShowEpisodesReportNoEffectDMCARemoveShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-show-episodes-report-no-effect-dmca-remove-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-show-episodes-report-no-effect-dmca-remove-show-force.failed"
           );
        }
        [MessageHandler("resolve-show-report-no-effect-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleResolveShowReportNoEffectUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveShowReportNoEffectUnpublishShowForceParameterDTO>();
                   await _podcastShowReportService.ResolveShowReportNoEffectUnpublishShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-show-report-no-effect-unpublish-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-show-report-no-effect-unpublish-show-force.failed"
           );
        }
        [MessageHandler("resolve-show-episodes-report-no-effect-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleResolveShowEpisodesReportNoEffectUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveShowEpisodesReportNoEffectUnpublishShowForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolveShowEpisodesReportNoEffectUnpublishShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-show-episodes-report-no-effect-unpublish-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-show-episodes-report-no-effect-unpublish-show-force.failed"
           );
        }
        [MessageHandler("resolve-channel-shows-report-no-effect-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleResolveChannelShowsReportNoEffectUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveChannelShowsReportNoEffectUnpublishChannelForceParameterDTO>();
                   await _podcastShowReportService.ResolveChannelShowsReportNoEffectUnpublishChannelForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-channel-shows-report-no-effect-unpublish-channel-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-channel-shows-report-no-effect-unpublish-channel-force.failed"
           );
        }
        [MessageHandler("resolve-channel-episodes-report-no-effect-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleResolveChannelEpisodesReportNoEffectUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveChannelEpisodesReportNoEffectUnpublishChannelForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolveChannelEpisodesReportNoEffectUnpublishChannelForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-channel-episodes-report-no-effect-unpublish-channel-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-channel-episodes-report-no-effect-unpublish-channel-force.failed"
           );
        }
        [MessageHandler("resolve-episode-report-no-effect-episode-deletion-force", SAGA_TOPIC)]
        public async Task HandleResolveEpisodeReportNoEffectEpisodeDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveEpisodeReportNoEffectEpisodeDeletionForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolveEpisodeReportNoEffectEpisodeDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-episode-report-no-effect-episode-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-episode-report-no-effect-episode-deletion-force.failed"
           );
        }
        [MessageHandler("resolve-show-report-no-effect-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleResolveShowReportNoEffectShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveShowReportNoEffectShowDeletionForceParameterDTO>();
                   await _podcastShowReportService.ResolveShowReportNoEffectShowDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-show-report-no-effect-show-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-show-report-no-effect-show-deletion-force.failed"
           );
        }
        [MessageHandler("resolve-show-episodes-report-no-effect-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleResolveShowEpisodesReportNoEffectShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveShowEpisodesReportNoEffectShowDeletionForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolveShowEpisodesReportNoEffectShowDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-show-episodes-report-no-effect-show-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-show-episodes-report-no-effect-show-deletion-force.failed"
           );
        }
        [MessageHandler("resolve-channel-shows-report-no-effect-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleResolveChannelShowsReportNoEffectChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveChannelShowsReportNoEffectChannelDeletionForceParameterDTO>();
                   await _podcastShowReportService.ResolveChannelShowsReportNoEffectChannelDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-channel-shows-report-no-effect-channel-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-channel-shows-report-no-effect-channel-deletion-force.failed"
           );
        }
        [MessageHandler("resolve-channel-episodes-report-no-effect-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleResolveChannelEpisodesReportNoEffectChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolveChannelEpisodesReportNoEffectChannelDeletionForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolveChannelEpisodesReportNoEffectChannelDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-channel-episodes-report-no-effect-channel-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-channel-episodes-report-no-effect-channel-deletion-force.failed"
           );
        }
        [MessageHandler("resolve-podcast-buddy-report-no-effect-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleResolvePodcastBuddyReportNoEffectTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolvePodcastBuddyReportNoEffectTerminatePodcasterForceParameterDTO>();
                   await _podcastBuddyReportService.ResolvePodcastBuddyReportNoEffectTerminatePodcasterForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-podcast-buddy-report-no-effect-terminate-podcaster-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-podcast-buddy-report-no-effect-terminate-podcaster-force.failed"
           );
        }
        [MessageHandler("resolve-podcaster-shows-report-no-effect-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleResolvePodcasterShowsReportNoEffectTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolvePodcasterShowsReportNoEffectTerminatePodcasterForceParameterDTO>();
                   await _podcastShowReportService.ResolvePodcasterShowsReportNoEffectTerminatePodcasterForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-podcaster-shows-report-no-effect-terminate-podcaster-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-podcaster-shows-report-no-effect-terminate-podcaster-force.failed"
           );
        }
        [MessageHandler("resolve-podcaster-episodes-report-no-effect-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForceParameterDTO>();
                   await _podcastEpisodeReportService.ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForceAsync(parameter, command);
                   _logger.LogInformation("Handled resolve-podcaster-episodes-report-no-effect-terminate-podcaster-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "resolve-podcaster-episodes-report-no-effect-terminate-podcaster-force.failed"
           );
        }
    }
}
