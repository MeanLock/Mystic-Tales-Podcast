using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.Attributes;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.AssignDMCAAccusationToStaff;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CancelDMCAAccusationReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateCounterNotice;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusation;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusationReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateLawsuitProof;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissChannelEpisodesDMCAChannelDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissChannelEpisodesDMCAUnpublishChannelForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissChannelShowsDMCACannelDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissChannelShowsDMCAUnpublishChannelForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissEpisodeDMCAEpisodeDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissEpisodeDMCAUnpublishEpisodeForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissOtherEpisodeDMCADMCARemoveEpisodeForced;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissOtherShowDMCADMCARemoveShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissPodcasterEpisodesDMCATerminatePodcasterForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissPodcasterShowsDMCATerminatePodcasterForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissShowDMCAShowDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissShowDMCAUnpublishShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissShowEpisodesDMCADMCARemoveShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissShowEpisodesDMCAShowDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.DismissShowEpisodesDMCAUnpublishShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.UpdateDMCAAccusationStatus;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.ValidateDMCAAccusationReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreatePodcastBuddyReport;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Models.Mail;
using ModerationService.BusinessLogic.Services.DbServices.DMCAServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Infrastructure.Services.Kafka;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.MessageHandlers
{
    public class DMCAManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly ILogger<DMCAManagementDomainMessageHandler> _logger;
        private readonly DMCAAccusationService _dmcaAccusationService;
        private readonly DMCANoticeService _dmcaNoticeService;
        private readonly CounterNoticeService _counterNoticeService;
        private readonly LawsuitProofService _lawsuitProofService;
        private const string SAGA_TOPIC = KafkaTopicEnum.DmcaManagementDomain;
        public DMCAManagementDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<DMCAManagementDomainMessageHandler> logger,
            DMCAAccusationService dmcaAccusationService,
            DMCANoticeService dmcaNoticeService,
            CounterNoticeService counterNoticeService,
            LawsuitProofService lawsuitProofService) : base(messagingService, kafkaProducerService, logger)
        {
            _logger = logger;
            _dmcaAccusationService = dmcaAccusationService;
            _dmcaNoticeService = dmcaNoticeService;
            _counterNoticeService = counterNoticeService;
            _lawsuitProofService = lawsuitProofService;
        }
        [MessageHandler("create-dmca-accusation", SAGA_TOPIC)]
        public async Task HandleCreateDMCAAccusationAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CreateDMCAAccusationParameterDTO>();
                   await _dmcaAccusationService.CreateDMCAAccusationAsync(parameter, command);
                   _logger.LogInformation("Handled create-dmca-accusation command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "create-dmca-accusation.failed"
           );
        }
        [MessageHandler("create-counter-notice", SAGA_TOPIC)]
        public async Task HandleCreateCounterNoticeAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CreateCounterNoticeParameterDTO>();
                   await _counterNoticeService.CreateCounterNoticeAsync(parameter, command);
                   _logger.LogInformation("Handled create-counter-notice command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "create-counter-notice.failed"
           );
        }
        [MessageHandler("create-lawsuit-proof", SAGA_TOPIC)]
        public async Task HandleCreateLawsuitProofAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CreateLawsuitProofParameterDTO>();
                   await _lawsuitProofService.CreateLawsuitProofAsync(parameter, command);
                   _logger.LogInformation("Handled create-lawsuit-proof command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "create-lawsuit-proof.failed"
           );
        }
        [MessageHandler("assign-dmca-accusation-to-staff", SAGA_TOPIC)]
        public async Task HandlerAssignDMCAAccusationToStaffAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<AssignDMCAAccusationToStaffParameterDTO>();
                   await _dmcaAccusationService.AssignDMCAAccusationToStaffAsync(parameter, command);
                   _logger.LogInformation("Handled assign-dmca-accusation-to-staff command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "assign-dmca-accusation-to-staff.failed"
           );
        }
        [MessageHandler("create-dmca-accusation-report", SAGA_TOPIC)]
        public async Task HandleCreateDMCAAccusationReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CreateDMCAAccusationReportParameterDTO>();
                   await _dmcaAccusationService.CreateDMCAAccusationReportAsync(parameter, command);
                   _logger.LogInformation("Handled create-dmca-accusation-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "create-dmca-accusation-report.failed"
           );
        }
        [MessageHandler("update-dmca-accusation-status", SAGA_TOPIC)]
        public async Task HandleUpdateDMCAAccusationStatusAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<UpdateDMCAAccusationStatusParameterDTO>();
                   await _dmcaAccusationService.UpdateDMCAAccusationStatusAsync(parameter, command);
                   _logger.LogInformation("Handled update-dmca-accusation-status command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "update-dmca-accusation-status.failed"
           );
        }
        [MessageHandler("validate-dmca-accusation-report", SAGA_TOPIC)]
        public async Task HandleValidateDMCAAccusationReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<ValidateDMCAAccusationReportParameterDTO>();
                   await _dmcaAccusationService.ValidateDMCAAccusationReportAsync(parameter, command);
                   _logger.LogInformation("Handled validate-dmca-accusation-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "validate-dmca-accusation-report.failed"
           );
        }
        [MessageHandler("cancel-dmca-accusation-report", SAGA_TOPIC)]
        public async Task HandleCancelDMCAAccusationReportAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<CancelDMCAAccusationReportParameterDTO>();
                   await _dmcaAccusationService.CancelDMCAAccusationReportAsync(parameter, command);
                   _logger.LogInformation("Handled cancel-dmca-accusation-report command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "cancel-dmca-accusation-report.failed"
           );
        }
        [MessageHandler("dismiss-other-episode-dmca-dmca-remove-episode-force", SAGA_TOPIC)]
        public async Task HandleDismissOtherEpisodeDMCA_DMCARemoveEpisodeForcedAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissOtherEpisodeDMCADMCARemoveEpisodeForcedParameterDTO>();
                   await _dmcaAccusationService.DismissOtherEpisodeDMCADMCARemoveEpisodeForcedAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-other-episode-dmca-dmca-remove-episode-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-other-episode-dmca-dmca-remove-episode-force.failed"
           );
        }
        [MessageHandler("dismiss-episode-dmca-unpublish-episode-force", SAGA_TOPIC)]
        public async Task HandleDismissEpisodeDMCA_UnpublishEpisodeForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissEpisodeDMCAUnpublishEpisodeForceParameterDTO>();
                   await _dmcaAccusationService.DismissEpisodeDMCAUnpublishEpisodeForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-episode-dmca-unpublish-episode-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-episode-dmca-unpublish-episode-force.failed"
           );
        }
        [MessageHandler("dismiss-other-show-dmca-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleDismissOtherShowDMCA_DMCARemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissOtherShowDMCADMCARemoveShowForceParameterDTO>();
                   await _dmcaAccusationService.DismissOtherShowDMCADMCARemoveShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-other-show-dmca-dmca-remove-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-other-show-dmca-dmca-remove-show-force.failed"
           );
        }
        [MessageHandler("dismiss-show-episodes-dmca-dmca-remove-show-force", SAGA_TOPIC)]
        public async Task HandleDismissShowEpisodesDMCA_DMCARemoveShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissShowEpisodesDMCADMCARemoveShowForceParameterDTO>();
                   await _dmcaAccusationService.DismissShowEpisodesDMCADMCARemoveShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-show-episodes-dmca-dmca-remove-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-show-episodes-dmca-dmca-remove-show-force.failed"
           );
        }
        [MessageHandler("dismiss-show-dmca-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleDismissShowDMCA_UnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissShowDMCAUnpublishShowForceParameterDTO>();
                   await _dmcaAccusationService.DismissShowDMCAUnpublishShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-show-dmca-unpublish-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-show-dmca-unpublish-show-force.failed"
           );
        }
        [MessageHandler("dismiss-show-episodes-dmca-unpublish-show-force", SAGA_TOPIC)]
        public async Task HandleDismissShowEpisodesDMCAUnpublishShowForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissShowEpisodesDMCAUnpublishShowForceParameterDTO>();
                   await _dmcaAccusationService.DismissShowEpisodesDMCAUnpublishShowForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-show-episodes-dmca-unpublish-show-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-show-episodes-dmca-unpublish-show-force.failed"
           );
        }
        [MessageHandler("dismiss-channel-shows-dmca-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleDismissChannelShowsDMCAUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissChannelShowsDMCAUnpublishChannelForceParameterDTO>();
                   await _dmcaAccusationService.DismissChannelShowsDMCAUnpublishChannelForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-channel-shows-dmca-unpublish-channel-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-channel-shows-dmca-unpublish-channel-force.failed"
           );
        }
        [MessageHandler("dismiss-channel-episodes-dmca-unpublish-channel-force", SAGA_TOPIC)]
        public async Task HandleDismissChannelEpisodesDMCAUnpublishChannelForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissChannelEpisodesDMCAUnpublishChannelForceParameterDTO>();
                   await _dmcaAccusationService.DismissChannelEpisodesDMCAUnpublishChannelForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-channel-episodes-dmca-unpublish-channel-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-channel-episodes-dmca-unpublish-channel-force.failed"
           );
        }
        [MessageHandler("dismiss-episode-dmca-episode-deletion-force", SAGA_TOPIC)]
        public async Task HandleDismissEpisodeDMCA_EpisodeDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissEpisodeDMCAEpisodeDeletionForceParameterDTO>();
                   await _dmcaAccusationService.DismissEpisodeDMCAEpisodeDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-episode-dmca-episode-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-episode-dmca-episode-deletion-force.failed"
           );
        }
        [MessageHandler("dismiss-show-dmca-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDismissShowDMCA_ShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissShowDMCAShowDeletionForceParameterDTO>();
                   await _dmcaAccusationService.DismissShowDMCAShowDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-show-dmca-show-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-show-dmca-show-deletion-force.failed"
           );
        }
        [MessageHandler("dismiss-show-episodes-dmca-show-deletion-force", SAGA_TOPIC)]
        public async Task HandleDismissShowEpisodesDMCA_ShowDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissShowEpisodesDMCAShowDeletionForceParameterDTO>();
                   await _dmcaAccusationService.DismissShowEpisodesDMCAShowDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-show-episodes-dmca-show-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-show-episodes-dmca-show-deletion-force.failed"
           );
        }
        [MessageHandler("dismiss-channel-shows-dmca-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDismissChannelShowsDMCAChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissChannelShowsDMCAChannelDeletionForceParameterDTO>();
                   await _dmcaAccusationService.DismissChannelShowsDMCAChannelDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-channel-shows-dmca-channel-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-channel-shows-dmca-channel-deletion-force.failed"
           );
        }
        [MessageHandler("dismiss-channel-episodes-dmca-channel-deletion-force", SAGA_TOPIC)]
        public async Task HandleDismissChannelEpisodesDMCAChannelDeletionForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissChannelEpisodesDMCAChannelDeletionForceParameterDTO>();
                   await _dmcaAccusationService.DismissChannelEpisodesDMCAChannelDeletionForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-channel-episodes-dmca-channel-deletion-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-channel-episodes-dmca-channel-deletion-force.failed"
           );
        }
        [MessageHandler("dismiss-podcaster-shows-dmca-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDismissPodcasterShowsDMCATerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissPodcasterShowsDMCATerminatePodcasterForceParameterDTO>();
                   await _dmcaAccusationService.DismissPodcasterShowsDMCATerminatePodcasterForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-podcaster-shows-dmca-terminate-podcaster-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-podcaster-shows-dmca-terminate-podcaster-force.failed"
           );
        }
        [MessageHandler("dismiss-podcaster-episodes-dmca-terminate-podcaster-force", SAGA_TOPIC)]
        public async Task HandleDismissPodcasterEpisodesDMCATerminatePodcasterForceAsync(string key, string messageJson)
        {
            await ExecuteSagaCommandMessageAsync(
               messageJson,
               async (command) =>
               {
                   var parameter = command.RequestData.ToObject<DismissPodcasterEpisodesDMCATerminatePodcasterForceParameterDTO>();
                   await _dmcaAccusationService.DismissPodcasterEpisodesDMCATerminatePodcasterForceAsync(parameter, command);
                   _logger.LogInformation("Handled dismiss-podcaster-episodes-dmca-terminate-podcaster-force command for SagaId: {SagaId}", command.SagaInstanceId);
               },
               responseTopic: SAGA_TOPIC,
               failedEmitMessage: "dismiss-podcaster-episodes-dmca-terminate-podcaster-force.failed"
           );
        }
    }
}
