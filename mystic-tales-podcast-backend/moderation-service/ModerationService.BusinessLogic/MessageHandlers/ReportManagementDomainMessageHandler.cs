using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.Attributes;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateEpisodeReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreatePodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateShowReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastShowReport;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Infrastructure.Services.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.MessageHandlers
{
    public class ReportManagementDomainMessageHandler : BaseSagaCommandMessageHandler
    {
        private readonly ILogger<ReportManagementDomainMessageHandler> _logger;
        private readonly PodcastBuddyReportService _podcastBuddyReportService;
        private readonly PodcastShowReportService _podcastShowReportService;
        private readonly PodcastEpisodeReportService _podcastEpisodeReportService;
        private const string SAGA_TOPIC = KafkaTopicEnum.ReportManagementDomain;
        public ReportManagementDomainMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger<ReportManagementDomainMessageHandler> logger,
            PodcastBuddyReportService podcastBuddyReportService,
            PodcastShowReportService podcastShowReportService,
            PodcastEpisodeReportService podcastEpisodeReportService) : base(messagingService, kafkaProducerService, logger)
        {
            _logger = logger;
            _podcastBuddyReportService = podcastBuddyReportService;
            _podcastShowReportService = podcastShowReportService;
            _podcastEpisodeReportService = podcastEpisodeReportService;
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
    }
}
