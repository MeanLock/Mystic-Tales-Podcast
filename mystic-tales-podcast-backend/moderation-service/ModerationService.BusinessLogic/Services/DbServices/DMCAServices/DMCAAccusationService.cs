using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.Details;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.ListItems;
using ModerationService.BusinessLogic.DTOs.DMCAReport.ListItems;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.AssignDMCAAccusationToStaff;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CancelDMCAAccusationReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusation;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusationReport;
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
using ModerationService.BusinessLogic.DTOs.Podcast;
using ModerationService.BusinessLogic.DTOs.Snippet;
using ModerationService.BusinessLogic.DTOs.SystemConfiguration;
using ModerationService.BusinessLogic.Enums.Account;
using ModerationService.BusinessLogic.Enums.DMCA;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Enums.Podcast;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Helpers.FileHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Models.Mail;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;
using ModerationService.Common.AppConfigurations.FilePath.interfaces;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.DataAccess.Repositories.interfaces;
using ModerationService.Infrastructure.Models.Kafka;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.Podcast;

namespace ModerationService.BusinessLogic.Services.DbServices.DMCAServices
{
    public class DMCAAccusationService
    {
        private readonly IGenericRepository<Dmcaaccusation> _dmcaAccusationGenericRepository;
        private readonly IGenericRepository<DmcaaccusationStatus> _dmcaAccusationStatusGenericRepository;
        private readonly IGenericRepository<DmcaaccusationStatusTracking> _dmcaAccusationStatusTrackingGenericRepository;
        private readonly IGenericRepository<DmcaaccusationConclusionReport> _dmcaAccusationConclusionReportGenericRepository;

        private readonly AccountCachingService _accountCachingService;
        private readonly DMCANoticeService _dmcaNoticeService;
        private readonly CounterNoticeService _counterNoticeService;
        private readonly LawsuitProofService _lawsuitProofService;
        private readonly MailOperationService _mailOperationService;

        private readonly IDMCAAccusationConfig _dmcaAccusationConfig;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;

        private readonly ILogger<DMCAAccusationService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public DMCAAccusationService(
            IGenericRepository<Dmcaaccusation> dmcaAccusationGenericRepository,
            IGenericRepository<DmcaaccusationStatus> dmcaAccusationStatusGenericRepository,
            IGenericRepository<DmcaaccusationStatusTracking> dmcaAccusationStatusTrackingGenericRepository,
            IGenericRepository<DmcaaccusationConclusionReport> dmcaAccusationConclusionReportGenericRepository,
            AccountCachingService accountCachingService,
            DMCANoticeService dmcaNoticeService,
            CounterNoticeService counterNoticeService,
            LawsuitProofService lawsuitProofService,
            MailOperationService mailOperationService,
            IDMCAAccusationConfig dmcaAccusationConfig,
            IFileValidationConfig fileValidationConfig,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            ILogger<DMCAAccusationService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper
            )
        {
            _dmcaAccusationGenericRepository = dmcaAccusationGenericRepository;
            _dmcaAccusationStatusGenericRepository = dmcaAccusationStatusGenericRepository;
            _dmcaAccusationStatusTrackingGenericRepository = dmcaAccusationStatusTrackingGenericRepository;
            _dmcaAccusationConclusionReportGenericRepository = dmcaAccusationConclusionReportGenericRepository;
            _accountCachingService = accountCachingService;
            _dmcaNoticeService = dmcaNoticeService;
            _counterNoticeService = counterNoticeService;
            _lawsuitProofService = lawsuitProofService;
            _mailOperationService = mailOperationService;
            _dmcaAccusationConfig = dmcaAccusationConfig;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<List<DMCAAccusationListItemResponseDTO>> GetAllDMCAAccusationForStaffOrAdminAsync(int? staffId, int roleId)
        {
            var query = await _dmcaAccusationGenericRepository.FindAll(
                predicate: null,
                includeFunc: function => function
                    .Include(da => da.DmcaaccusationStatusTrackings)
                    .ThenInclude(dast => dast.DmcaAccusationStatus)
                ).ToListAsync();
            if (roleId == (int)RoleEnum.Staff)
            {
                query = query.Where(pbrrs => pbrrs.AssignedStaff == staffId).ToList();
            }
            var dmcaAccusation = (await Task.WhenAll(query.Select(async pbrrs =>
            {
                PodcastEpisodeWithShowDTO? episode = null;
                PodcastShowDTO? show = null;
                if (pbrrs.PodcastEpisodeId != null)
                {
                    episode = await GetPodcastEpisodeWithShow(pbrrs.PodcastEpisodeId.Value);
                }
                if(pbrrs.PodcastShowId != null)
                {
                    show = await GetPodcastShow(pbrrs.PodcastShowId.Value);
                }
                if(episode == null && show == null)
                {
                    _logger.LogWarning("DMCA Accusation {Id} has no associated episode or show", pbrrs.Id);
                    return null;
                }
                AccountStatusCache? staff = null;
                if (pbrrs.AssignedStaff.HasValue)
                {
                    staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff.Value);
                }
                AccountStatusCache? podcaster = null;
                if(episode != null)
                {
                    if(episode.PodcastShow != null)
                    {
                        podcaster = await _accountCachingService.GetAccountStatusCacheById(episode.PodcastShow.PodcasterId);
                    }
                    else
                    {
                        var showOfEpisode = await GetPodcastShow(episode.PodcastShowId);
                        podcaster = await _accountCachingService.GetAccountStatusCacheById(showOfEpisode.PodcasterId);
                    }
                    //podcaster = await _accountCachingService.GetAccountStatusCacheById(episode.PodcastShow.PodcasterId);
                }
                if(show != null && podcaster == null)
                {
                    podcaster = await _accountCachingService.GetAccountStatusCacheById(show.PodcasterId);
                }
                return new DMCAAccusationListItemResponseDTO()
                {
                    Id = pbrrs.Id,
                    AccuserEmail = pbrrs.AccuserEmail,
                    AccuserPhone = pbrrs.AccuserPhone,
                    AccuserFullName = pbrrs.AccuserFullName,
                    DismissReason = pbrrs.DismissReason,
                    PodcastShow = show != null
                        ? new DMCAPodcastShowSnippetResponseDTO()
                        {
                            Id = show.Id,
                            Name = show.Name,
                            MainImageFileKey = show.MainImageFileKey,
                            PodcasterName = podcaster.FullName
                        }
                        : null,
                    PodcastEpisode = episode != null
                        ? new DMCAPodcastEpisodeSnippetResponseDTO()
                        {
                            Id = episode.Id,
                            Name = episode.Name,
                            MainImageFileKey = episode.MainImageFileKey,
                            PodcasterName = podcaster.FullName
                        }
                        : null,
                    AssignedStaff = staff != null
                        ? new AssignedStaffSnippetResponseDTO()
                        {
                            Id = staff.Id,
                            FullName = staff.FullName,
                            Email = staff.Email,
                            MainImageFileKey = staff.MainImageFileKey
                        }
                        : null,
                    CreatedAt = pbrrs.CreatedAt,
                    UpdatedAt = pbrrs.UpdatedAt,
                    CurrentStatus = new DMCAAccusationStatusDTO()
                    {
                        Id = pbrrs.DmcaaccusationStatusTrackings
                            .OrderByDescending(dast => dast.CreatedAt)
                            .First()
                            .DmcaAccusationStatus.Id,
                        Name = pbrrs.DmcaaccusationStatusTrackings
                            .OrderByDescending(dast => dast.CreatedAt)
                            .First()
                            .DmcaAccusationStatus.Name
                    }
                };
            }))).ToList();
            return dmcaAccusation;
        }
        public async Task CreateDMCAAccusationAsync(CreateDMCAAccusationParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                var processedFiles = new List<string>();
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    if(parameter.PodcastShowId != null)
                    {
                        var showValidation = await ValidateShow(parameter.PodcastShowId.Value);
                        if (!showValidation.isValid)
                        {
                            throw new Exception(showValidation.errorMessage);
                        }
                    }

                    if(parameter.PodcastEpisodeId != null)
                    {
                        var episodeValidation = await ValidateEpisode(parameter.PodcastEpisodeId.Value);
                        if (!episodeValidation.isValid)
                        {
                            throw new Exception(episodeValidation.errorMessage);
                        }
                    }

                    var newDmcaAccusation = new Dmcaaccusation
                    {
                        AccuserEmail = parameter.AccuserEmail,
                        AccuserPhone = parameter.AccuserPhone,
                        AccuserFullName = parameter.AccuserFullName,
                        PodcastShowId = parameter.PodcastShowId,
                        PodcastEpisodeId = parameter.PodcastEpisodeId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    var createdDmcaAccusation = await _dmcaAccusationGenericRepository.CreateAsync(newDmcaAccusation);

                    var newDmcaNotice = new Dmcanotice
                    {
                        DmcaAccusationId = createdDmcaAccusation.Id,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    var createDmcaNotice = await _dmcaNoticeService.CreateDMCANoticeAsync(newDmcaNotice);

                    if(createDmcaNotice == null)
                    {
                        throw new Exception("Failed to create dmca notice");
                    }

                    var createDMCANoticeAttachFile = new List<string>();

                    if(parameter.DMCANoticeAttachFileKeys == null || parameter.DMCANoticeAttachFileKeys.Count == 0)
                    {
                        throw new Exception("At least one dmca notice attach file is required");
                    }
                    // Process each file
                    foreach (var attachFileKey in parameter.DMCANoticeAttachFileKeys)
                    {

                        var newDMCANoticeAttachFile = new DmcanoticeAttachFile()
                        {
                            DmcaNoticeId = createDmcaNotice.Id,
                            AttachFileKey = "",
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        };

                        var dmcanoticeAttachFile = await _dmcaNoticeService.CreateDMCANoticeAttachFile(newDMCANoticeAttachFile);

                        if(dmcanoticeAttachFile == null)
                        {
                            throw new Exception("Failed to create dmca notice attach file");
                        }

                        var folderPath = _filePathConfig.DMCA_ACCUSATION_FILE_PATH + "\\" + createdDmcaAccusation.Id;
                        if (attachFileKey != null && attachFileKey != "")
                        {
                            var newAttachFileKey = FilePathHelper.CombinePaths(folderPath, $"{dmcanoticeAttachFile.Id}_dmca_notice{FilePathHelper.GetExtension(attachFileKey)}");
                            await _fileIOHelper.CopyFileToFileAsync(attachFileKey, newAttachFileKey);
                            processedFiles.Add(newAttachFileKey);
                            await _fileIOHelper.DeleteFileAsync(attachFileKey);
                            dmcanoticeAttachFile.AttachFileKey = newAttachFileKey;
                            await _dmcaNoticeService.UpdateDMCANoticeAttachFile(dmcanoticeAttachFile);
                        }

                        createDMCANoticeAttachFile.Add(dmcanoticeAttachFile.AttachFileKey);
                    }

                    var newDMCAAccusationStatusTracking = new DmcaaccusationStatusTracking
                    {
                        DmcaAccusationId = createdDmcaAccusation.Id,
                        DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(newDMCAAccusationStatusTracking);

                    //Send email to Accuser
                    var accuserMailSendingRequestData = JObject.FromObject(new
                    {
                        SendModerationServiceEmailInfo = new
                        {
                            MailTypeName = "DMCANoticePending",
                            ToEmail = createdDmcaAccusation.AccuserEmail,
                            MailObject = new DMCANoticePendingMailViewModel
                            {
                                AccuserEmail = createdDmcaAccusation.AccuserEmail,
                                AccuserFullName = createdDmcaAccusation.AccuserFullName,
                                CreatedDate = _dateHelper.GetNowByAppTimeZone(),
                            }
                        }
                    });
                    var accuserMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: accuserMailSendingRequestData,
                        sagaInstanceId: null,
                        messageName: "moderation-service-mail-sending-flow");
                    await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow);

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "DMCAAccusationId", createdDmcaAccusation.Id },
                        { "DMCANoticeId", createDmcaNotice.Id },
                        { "AccuserEmail", createdDmcaAccusation.AccuserEmail },
                        { "AccuserPhone", createdDmcaAccusation.AccuserPhone },
                        { "AccuserFullName", createdDmcaAccusation.AccuserFullName },
                        { "PodcastShowId", createdDmcaAccusation.PodcastShowId },
                        { "PodcastEpisodeId", createdDmcaAccusation.PodcastEpisodeId },
                        //{ "DMCAAttachFileKeys", JArray.FromObject(createDMCANoticeAttachFile) },
                        { "CreatedAt", createdDmcaAccusation.CreatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully create dmca accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    foreach (var processedFile in processedFiles)
                    {
                        try
                        {
                            await _fileIOHelper.DeleteFileAsync(processedFile);
                            _logger.LogInformation("Cleaned up processed file: {FilePath}", processedFile);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, "Failed to clean up processed file: {FilePath}", processedFile);
                        }
                    }

                    foreach (var attachFile in parameter.DMCANoticeAttachFileKeys)
                    {
                        if (attachFile != null && attachFile != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(attachFile);
                        }
                    }
                    _logger.LogError(ex, "Error occurred while creating dmca accusation for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create dmca accusation failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Create dmca accusation failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CreateDMCAAccusationReportAsync(CreateDMCAAccusationReportParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                var processedFiles = new List<string>();
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var exisitingDmcaAccusationReport = await _dmcaAccusationConclusionReportGenericRepository.FindAll(
                        predicate: dar => dar.DmcaAccusationId == parameter.DmcaAccusationId && (dar.CompletedAt == null && dar.IsRejected == null && dar.CancelledAt == null)
                        ).FirstOrDefaultAsync();

                    if(exisitingDmcaAccusationReport != null)
                    {
                        throw new Exception("A current active DMCA Accusation Report already exists for this DMCA Accusation");
                    }

                    var newDmcaAccusationReport = new DmcaaccusationConclusionReport
                    {
                        DmcaAccusationId = parameter.DmcaAccusationId,
                        DmcaAccusationConclusionReportTypeId = parameter.DmcaAccusationConclusionReportTypeId,
                        Description = parameter.Description,
                        InvalidReason = parameter.InvalidReason,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    var createdDmcaAccusationReport = await _dmcaAccusationConclusionReportGenericRepository.CreateAsync(newDmcaAccusationReport);

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "DMCAAccusationReportId", createdDmcaAccusationReport.Id },
                        { "DMCAAccusationId", createdDmcaAccusationReport.DmcaAccusationId },
                        { "DmcaAccusationConclusionReportTypeId", createdDmcaAccusationReport.DmcaAccusationConclusionReportTypeId },
                        { "Description", createdDmcaAccusationReport.Description },
                        { "InvalidReason", createdDmcaAccusationReport.InvalidReason },
                        { "CreatedAt", createdDmcaAccusationReport.CreatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully create dmca accusation report for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating dmca accusation report for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Creating dmca accusation report failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Creating dmca accusation report failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<DMCAAccusationDetailResponseDTO> GetDMCAAccusationByIdForStaffOrAdminAsync(int dmcaAccusationId, int? accountId, int roleId)
        {
            var query = _dmcaAccusationGenericRepository.FindAll(
               predicate: null,
               includeFunc: function => function
                   .Include(da => da.DmcaaccusationStatusTrackings)
                   .ThenInclude(dast => dast.DmcaAccusationStatus)
               )
                .Where(da => da.Id == dmcaAccusationId);

            if (roleId == (int)RoleEnum.Staff)
            {
                query = query.Where(pbrrs => pbrrs.AssignedStaff == accountId);
            }

            // Materialize the entity first, then use async/await for further processing
            var da = await query.FirstOrDefaultAsync();
            if (da == null)
                return null;

            PodcastEpisodeWithShowDTO? episode = null;
            PodcastShowDTO? show = null;
            if (da.PodcastEpisodeId != null)
            {
                episode = await GetPodcastEpisodeWithShow(da.PodcastEpisodeId.Value);
            }
            if(da.PodcastShowId != null)
            {
                show = await GetPodcastShow(da.PodcastShowId.Value);
            }
            if(episode == null && show == null)
            {
                _logger.LogWarning("DMCA Accusation {Id} has no associated episode or show", da.Id);
                return null;
            }
            AccountStatusCache? staff = null;
            if (da.AssignedStaff.HasValue)
            {
                staff = await _accountCachingService.GetAccountStatusCacheById(da.AssignedStaff.Value);
            }
            AccountStatusCache? podcaster = null;
            if (episode != null)
            {
                if(episode.PodcastShow != null)
                {
                    podcaster = await _accountCachingService.GetAccountStatusCacheById(episode.PodcastShow.PodcasterId);
                }
                else
                {
                    var showOfEpisode = await GetPodcastShow(episode.PodcastShowId);
                    podcaster = await _accountCachingService.GetAccountStatusCacheById(showOfEpisode.PodcasterId);
                }
            }
            if (show != null && podcaster == null)
            {
                podcaster = await _accountCachingService.GetAccountStatusCacheById(show.PodcasterId);
            }
            var dmcaNoticeList = await _dmcaNoticeService.GetDMCANoticeByDMCAAccusationId(da.Id);
            var counterNoticeList = await _counterNoticeService.GetCounterNoticeByDMCAAccusationId(da.Id);
            var lawsuitProofList = await _lawsuitProofService.GetLawsuitProofByDMCAAccusationId(da.Id);
            return new DMCAAccusationDetailResponseDTO()
            {
                Id = da.Id,
                AccuserEmail = da.AccuserEmail,
                AccuserPhone = da.AccuserPhone,
                AccuserFullName = da.AccuserFullName,
                DismissReason = da.DismissReason,
                PodcastShow = show != null
                    ? new DMCAPodcastShowSnippetResponseDTO()
                    {
                        Id = show.Id,
                        Name = show.Name,
                        MainImageFileKey = show.MainImageFileKey,
                        PodcasterName = podcaster.FullName
                    }
                    : null,
                PodcastEpisode = episode != null
                    ? new DMCAPodcastEpisodeSnippetResponseDTO()
                    {
                        Id = episode.Id,
                        Name = episode.Name,
                        MainImageFileKey = episode.MainImageFileKey,
                        PodcasterName = podcaster.FullName
                    }
                    : null,
                AssignedStaff = staff != null
                    ? new AssignedStaffSnippetResponseDTO()
                    {
                        Id = staff.Id,
                        FullName = staff.FullName,
                        Email = staff.Email,
                        MainImageFileKey = staff.MainImageFileKey
                    }
                    : null,
                CreatedAt = da.CreatedAt,
                UpdatedAt = da.UpdatedAt,
                CurrentStatus = new DMCAAccusationStatusDTO()
                {
                    Id = da.DmcaaccusationStatusTrackings
                        .OrderByDescending(dast => dast.CreatedAt)
                        .First()
                        .DmcaAccusationStatus.Id,
                    Name = da.DmcaaccusationStatusTrackings
                        .OrderByDescending(dast => dast.CreatedAt)
                        .First()
                        .DmcaAccusationStatus.Name
                },
                DMCANotice = dmcaNoticeList,
                CounterNotice = counterNoticeList,
                LawsuitProof = lawsuitProofList
            };
        }
        public async Task AssignDMCAAccusationToStaffAsync(AssignDMCAAccusationToStaffParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var staffAccount = await _accountCachingService.GetAccountStatusCacheById(parameter.AccountId);
                    if(staffAccount == null || staffAccount.RoleId != (int)RoleEnum.Staff)
                    {
                        throw new Exception($"Staff account with Id: {parameter.AccountId} not found or is not a staff account");
                    }
                    if(staffAccount.DeactivatedAt != null)
                    {
                        throw new Exception($"Staff account with Id: {parameter.AccountId} is deactivated at {staffAccount.DeactivatedAt}, cannot be assigned to dmca accusation");
                    }

                    var dmcaAccusation = await _dmcaAccusationGenericRepository.FindByIdAsync(parameter.DMCAAccusationId);
                    dmcaAccusation.AssignedStaff = parameter.AccountId;
                    dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    var createdDmcaAccusation =  await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "DMCAAccusationId", createdDmcaAccusation.Id },
                        { "StaffAccountId", createdDmcaAccusation.AssignedStaff },
                        { "UpdatedAt", createdDmcaAccusation.UpdatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully assign staff to dmca accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while assigning staff to dmca accusation for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Assign staff to dmca accusation failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Assign staff to dmca accusation failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task UpdateDMCAAccusationStatusAsync(UpdateDMCAAccusationStatusParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                var processedFiles = new List<string>();
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var config = await GetActiveSystemConfigProfile();
                    if(config == null)
                    {
                        throw new Exception("Active system config profile not found");
                    }

                    var dmcaAccusation = await _dmcaAccusationGenericRepository.FindByIdAsync(parameter.DMCAAccusationId,
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings)
                        .Include(da => da.Dmcanotices)
                        .Include(da => da.CounterNotices)
                        .Include(da => da.LawsuitProofs));

                    if (dmcaAccusation == null)
                    {
                        throw new Exception($"DMCA Accusation with Id: {parameter.DMCAAccusationId} not found");
                    }
                    if(dmcaAccusation.ResolvedAt != null)
                    {
                        throw new Exception($"DMCA Accusation with Id: {parameter.DMCAAccusationId} is already resolved at {dmcaAccusation.ResolvedAt}, no further status update is allowed");
                    }

                    var show = new PodcastShowDTO();
                    var episode = new PodcastEpisodeWithShowDTO();
                    var podcasterId = 0;
                    if (dmcaAccusation.PodcastShowId != null)
                    {
                        show = await GetPodcastShow(dmcaAccusation.PodcastShowId.Value);
                        podcasterId = show.PodcasterId;
                    }
                    if (dmcaAccusation.PodcastEpisodeId != null)
                    {
                        episode = await GetPodcastEpisodeWithShow(dmcaAccusation.PodcastEpisodeId.Value);
                        if(episode.PodcastShow != null)
                        {
                            podcasterId = episode.PodcastShow.PodcasterId;
                        }
                        else
                        {
                            var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                            podcasterId = showFromEpisode.PodcasterId;
                        }
                    }
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(podcasterId);

                    var accusedId = 0;
                    switch (parameter.DMCAAccusationAction)
                    {
                        case (int)DMCAAccusationQueryEnum.VALID_DMCA_NOTICE:
                            //Validate Staff Account
                            if (dmcaAccusation.AssignedStaff != parameter.AccountId)
                            {
                                throw new Exception($"The logged in staff Id: {parameter.AccountId} is not authorized to update this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                            }

                            //Validate DMCA Accusation status
                            var currentStatus1 = dmcaAccusation.DmcaaccusationStatusTrackings
                                .OrderByDescending(dast => dast.CreatedAt)
                                .First()
                                .DmcaAccusationStatusId;
                            if (currentStatus1 != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation with Pending Dmca Notice Review status can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }
                            if (parameter.DMCAAccusationTakenDownReasonEnum == null || parameter.DMCAAccusationTakenDownReasonEnum == 0)
                            {
                                throw new Exception("DMCA Accusation Taken Down Reason is required for Valid DMCA Notice action");
                            }
                            //Send TakeDownFlow
                            var takeDownFlowMessage = new JObject
                            {
                                { "PodcastShowId", dmcaAccusation.PodcastShowId ?? null },
                                { "PodcastEpisodeId", dmcaAccusation.PodcastEpisodeId ?? null},
                                { "TakenDownReason", Enum.GetName((DMCATakeDownReasonEnum)parameter.DMCAAccusationTakenDownReasonEnum) }
                            };
                            var takeDownMessageName = "content-dmca-takedown-flow";
                            var sagaTakeDownStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ContentManagementDomain,
                                requestData: takeDownFlowMessage,
                                sagaInstanceId: null,
                                messageName: takeDownMessageName);
                            await _messagingService.SendSagaMessageAsync(sagaTakeDownStartSagaTriggerMessage, command.SagaInstanceId.ToString());
                            _logger.LogInformation("Started content dmca takedown flow for DMCA Accusation Id: {DMCAAccusationId}", dmcaAccusation.Id);

                            List<string> attachmentFileUrls = new List<string>();
                            foreach (var attachmentFile in parameter.AttachmentFileKeys)
                            {
                                var folderPath = _filePathConfig.DMCA_ACCUSATION_FILE_PATH + "\\" + dmcaAccusation.Id + "\\" + "mailing_files";
                                if (attachmentFile != null && attachmentFile != "")
                                {
                                    var attachmentFileKey = FilePathHelper.CombinePaths(folderPath, $"{Guid.NewGuid()}_dmca_notice{FilePathHelper.GetExtension(attachmentFile)}");
                                    await _fileIOHelper.CopyFileToFileAsync(attachmentFile, attachmentFileKey);
                                    processedFiles.Add(attachmentFileKey);
                                    await _fileIOHelper.DeleteFileAsync(attachmentFile);
                                    var url = await _fileIOHelper.GeneratePresignedUrlAsync(attachmentFileKey, ConvertDaysToSeconds(_dmcaAccusationConfig.DMCANoticeResponseTime));
                                    if (url != null)
                                    {
                                        attachmentFileUrls.Add(url);
                                    }
                                }
                            }

                            //Send Confirm email to Accuser
                            var accuserMailSendingRequestData = JObject.FromObject(new
                            {
                                SendModerationServiceEmailInfo = new
                                {
                                    MailTypeName = "DMCANoticeValidToAccuser",
                                    ToEmail = dmcaAccusation.AccuserEmail,
                                    MailObject = new DMCANoticeValidToAccuserMailViewModel
                                    {
                                        AccuserEmail = dmcaAccusation.AccuserEmail,
                                        AccuserFullName = dmcaAccusation.AccuserFullName,
                                        ValidatedAt = _dateHelper.GetNowByAppTimeZone(),
                                    }
                                }
                            });
                            var accuserMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ReportManagementDomain,
                                requestData: accuserMailSendingRequestData,
                                sagaInstanceId: null,
                                messageName: "moderation-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow);

                            //Send Email to Accused
                            var accusedMailSendingRequestData = JObject.FromObject(new
                            {
                                SendModerationServiceEmailInfo = new
                                {
                                    MailTypeName = "DMCANoticeValidToAccused",
                                    ToEmail = podcaster.Email,
                                    MailObject = new DMCANoticeValidToAccusedMailViewModel
                                    {
                                        PodcasterEmail = podcaster.Email,
                                        PodcasterFullName = podcaster.FullName,
                                        TimeToResponse = 14,
                                        ValidatedAt = _dateHelper.GetNowByAppTimeZone(),
                                        AttachmentFileUrls = attachmentFileUrls
                                    }
                                }
                            });
                            var accusedMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ReportManagementDomain,
                                requestData: accusedMailSendingRequestData,
                                sagaInstanceId: null,
                                messageName: "moderation-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow);

                            var dmcaNotice = dmcaAccusation.Dmcanotices.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                            dmcaNotice.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                            dmcaNotice.ValidatedBy = parameter.AccountId;
                            await _dmcaNoticeService.UpdateDMCANoticeAsync(dmcaNotice);

                            //Switch Status
                            var takedownDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.ValidDMCANotice,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(takedownDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);

                            //Update DMCA Notice
                            var dmcaNoticeUpdate = dmcaAccusation.Dmcanotices.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                            dmcaNoticeUpdate.IsValid = true;
                            dmcaNoticeUpdate.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                            dmcaNoticeUpdate.ValidatedBy = parameter.AccountId;
                            await _dmcaNoticeService.UpdateDMCANoticeAsync(dmcaNoticeUpdate);

                            break;
                        case (int)DMCAAccusationQueryEnum.VALID_DMCA_COUNTER_NOTICE:
                            //Validate Staff Account
                            if (dmcaAccusation.AssignedStaff != parameter.AccountId)
                            {
                                throw new Exception($"The logged in staff Id: {parameter.AccountId} is not authorized to update this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                            }

                            //Validate DMCA Accusation status
                            var currentStatus2 = dmcaAccusation.DmcaaccusationStatusTrackings
                                .OrderByDescending(dast => dast.CreatedAt)
                                .First()
                                .DmcaAccusationStatusId;
                            if (currentStatus2 != (int)DMCAAccusationStatusEnum.ValidDMCANotice)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation with Valid DMCA Notice status can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send TakeDownFlow
                            //var takeDownFlowMessage2 = new JObject
                            //{
                            //    { "PodcastShowId", dmcaAccusation.PodcastShowId ?? null },
                            //    { "PodcastEpisodeId", dmcaAccusation.PodcastEpisodeId ?? null},
                            //    { "TakenDownReason", Enum.GetName(DMCATakeDownReasonEnum.LawsuitDMCA) }
                            //};
                            //var takeDownMessageName2 = "content-dmca-takedown-flow";
                            //var sagaTakeDownStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //    topic: KafkaTopicEnum.ContentManagementDomain,
                            //    requestData: takeDownFlowMessage2,
                            //    sagaInstanceId: null,
                            //    messageName: takeDownMessageName2);
                            //await _messagingService.SendSagaMessageAsync(sagaTakeDownStartSagaTriggerMessage2);
                            //_logger.LogInformation("Started content dmca takedown flow for DMCA Accusation Id: {DMCAAccusationId}", dmcaAccusation.Id);

                            List<string> attachmentFileUrls2 = new List<string>();
                            foreach (var attachmentFile in parameter.AttachmentFileKeys)
                            {
                                var folderPath = _filePathConfig.DMCA_ACCUSATION_FILE_PATH + "\\" + dmcaAccusation.Id + "\\" + "mailing_files";
                                if (attachmentFile != null && attachmentFile != "")
                                {
                                    var attachmentFileKey = FilePathHelper.CombinePaths(folderPath, $"{Guid.NewGuid()}_counter_notice{FilePathHelper.GetExtension(attachmentFile)}");
                                    await _fileIOHelper.CopyFileToFileAsync(attachmentFile, attachmentFileKey);
                                    processedFiles.Add(attachmentFileKey);
                                    await _fileIOHelper.DeleteFileAsync(attachmentFile);
                                    var url = await _fileIOHelper.GeneratePresignedUrlAsync(attachmentFileKey, ConvertDaysToSeconds(_dmcaAccusationConfig.DMCANoticeResponseTime));
                                    if (url != null)
                                    {
                                        attachmentFileUrls2.Add(url);
                                    }
                                }
                            }

                            //Send Email to Accuser
                            var accuserMailSendingRequestData2 = JObject.FromObject(new
                            {
                                SendModerationServiceEmailInfo = new
                                {
                                    MailTypeName = "DMCACounterNoticeValidToAccuser",
                                    ToEmail = dmcaAccusation.AccuserEmail,
                                    MailObject = new DMCACounterNoticeValidToAccuserMailViewModel
                                    {
                                        AccuserEmail = dmcaAccusation.AccuserEmail,
                                        AccuserFullName = dmcaAccusation.AccuserFullName,
                                        ValidatedAt = _dateHelper.GetNowByAppTimeZone(),
                                        TimeToResponse = 14,
                                        AttachmentFileUrls = attachmentFileUrls2
                                    }
                                }
                            });
                            var accuserMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ReportManagementDomain,
                                requestData: accuserMailSendingRequestData2,
                                sagaInstanceId: null,
                                messageName: "moderation-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow2);

                            //Send Confirm email to Accused
                            var accusedMailSendingRequestData2 = JObject.FromObject(new
                            {
                                SendModerationServiceEmailInfo = new
                                {
                                    MailTypeName = "DMCACounterNoticeValidToAccused",
                                    ToEmail = dmcaAccusation.AccuserEmail,
                                    MailObject = new DMCACounterNoticeValidToAccusedMailViewModel
                                    {
                                        PodcasterEmail = podcaster.Email,
                                        PodcasterFullName = podcaster.FullName,
                                        ValidatedAt = _dateHelper.GetNowByAppTimeZone()
                                    }
                                }
                            });
                            var accusedMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ReportManagementDomain,
                                requestData: accusedMailSendingRequestData2,
                                sagaInstanceId: null,
                                messageName: "moderation-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow2);

                            var counterNotice = dmcaAccusation.CounterNotices.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                            counterNotice.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                            counterNotice.ValidatedBy = parameter.AccountId;
                            await _counterNoticeService.UpdateCounterNotice(counterNotice);

                            //Switch Status
                            var rejectDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.ValidCounterNotice,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(rejectDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);

                            //Update Counter Notice
                            var counterNoticeUpdate = dmcaAccusation.CounterNotices.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                            counterNoticeUpdate.IsValid = true;
                            counterNoticeUpdate.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                            counterNoticeUpdate.ValidatedBy = parameter.AccountId;
                            await _counterNoticeService.UpdateCounterNotice(counterNoticeUpdate);

                            break;
                        case (int)DMCAAccusationQueryEnum.VALID_LAWSUIT_PROOF:
                            //Validate Staff Account
                            if (dmcaAccusation.AssignedStaff != parameter.AccountId)
                            {
                                throw new Exception($"The logged in staff Id: {parameter.AccountId} is not authorized to update this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                            }

                            //Validate DMCA Accusation status
                            var currentStatus3 = dmcaAccusation.DmcaaccusationStatusTrackings
                                .OrderByDescending(dast => dast.CreatedAt)
                                .First()
                                .DmcaAccusationStatusId;
                            if (currentStatus3 != (int)DMCAAccusationStatusEnum.ValidCounterNotice)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation with Valid Counter Notice can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send TakeDownFlow
                            //var takeDownFlowMessage3 = new JObject
                            //{
                            //    { "PodcastShowId", dmcaAccusation.PodcastShowId ?? null },
                            //    { "PodcastEpisodeId", dmcaAccusation.PodcastEpisodeId ?? null},
                            //    { "TakenDownReason", Enum.GetName(DMCATakeDownReasonEnum.LawsuitDMCA) }
                            //};
                            //var takeDownMessageName3 = "content-dmca-takedown-flow";
                            //var sagaTakeDownStartSagaTriggerMessage3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //    topic: KafkaTopicEnum.ContentManagementDomain,
                            //    requestData: takeDownFlowMessage3,
                            //    sagaInstanceId: null,
                            //    messageName: takeDownMessageName3);
                            //await _messagingService.SendSagaMessageAsync(sagaTakeDownStartSagaTriggerMessage3);
                            //_logger.LogInformation("Started content dmca takedown flow for DMCA Accusation Id: {DMCAAccusationId}", dmcaAccusation.Id);

                            List<string> attachmentFileUrls3 = new List<string>();
                            foreach (var attachmentFile in parameter.AttachmentFileKeys)
                            {
                                var folderPath = _filePathConfig.DMCA_ACCUSATION_FILE_PATH + "\\" + dmcaAccusation.Id + "\\" + "mailing_files";
                                if (attachmentFile != null && attachmentFile != "")
                                {
                                    var attachmentFileKey = FilePathHelper.CombinePaths(folderPath, $"{Guid.NewGuid()}_lawsuit_document{FilePathHelper.GetExtension(attachmentFile)}");
                                    await _fileIOHelper.CopyFileToFileAsync(attachmentFile, attachmentFileKey);
                                    processedFiles.Add(attachmentFileKey);
                                    await _fileIOHelper.DeleteFileAsync(attachmentFile);
                                    var url = await _fileIOHelper.GeneratePresignedUrlAsync(attachmentFileKey, ConvertDaysToSeconds(_dmcaAccusationConfig.DMCANoticeResponseTime));
                                    if (url != null)
                                    {
                                        attachmentFileUrls3.Add(url);
                                    }
                                }
                            }

                            //Send Confirm Email to Accuser
                            var accuserMailSendingRequestData3 = JObject.FromObject(new
                            {
                                SendModerationServiceEmailInfo = new
                                {
                                    MailTypeName = "DMCALawsuitProofValidToAccuser",
                                    ToEmail = dmcaAccusation.AccuserEmail,
                                    MailObject = new DMCALawsuitProofValidToAccuserMailViewModel
                                    {
                                        AccuserEmail = dmcaAccusation.AccuserEmail,
                                        AccuserFullName = dmcaAccusation.AccuserFullName,
                                        ValidatedAt = _dateHelper.GetNowByAppTimeZone()
                                    }
                                }
                            });
                            var accuserMailSendingFlow3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ReportManagementDomain,
                                requestData: accuserMailSendingRequestData3,
                                sagaInstanceId: null,
                                messageName: "moderation-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow3);

                            //Send email to Accused
                            var accusedMailSendingRequestData3 = JObject.FromObject(new
                            {
                                SendModerationServiceEmailInfo = new
                                {
                                    MailTypeName = "DMCALawsuitProofValidToAccused",
                                    ToEmail = podcaster.Email,
                                    MailObject = new DMCALawsuitProofValidToAccusedMailViewModel
                                    {
                                        PodcasterEmail = podcaster.Email,
                                        PodcasterFullName = podcaster.FullName,
                                        AttachmentFileUrls = attachmentFileUrls3
                                    }
                                }
                            });
                            var accusedMailSendingFlow3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ReportManagementDomain,
                                requestData: accusedMailSendingRequestData3,
                                sagaInstanceId: null,
                                messageName: "moderation-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow3);

                            var lawsuitProof = dmcaAccusation.LawsuitProofs.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                            lawsuitProof.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                            lawsuitProof.ValidatedBy = parameter.AccountId;
                            await _lawsuitProofService.UpdateLawsuitProof(lawsuitProof);
                            //Switch Status
                            var withdrawDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.ValidLawsuitProof,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(withdrawDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);

                            //Update Lawsuit Proof
                            var lawsuitProofUpdate = dmcaAccusation.LawsuitProofs.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                            lawsuitProofUpdate.IsValid = true;
                            lawsuitProofUpdate.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                            lawsuitProofUpdate.ValidatedBy = parameter.AccountId;
                            await _lawsuitProofService.UpdateLawsuitProof(lawsuitProofUpdate);

                            break;
                        default:
                            throw new Exception("DMCAAccusationAction not recognize");
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                        {
                            { "StaffAccountId", dmcaAccusation.AssignedStaff },
                            { "DMCAAccusationId", dmcaAccusation.Id },
                            { "DMCAAccusationAction", Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction) },
                            { "UpdatedAt", dmcaAccusation.UpdatedAt }
                        };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Update DMCA Accusation status for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    foreach (var processedFile in processedFiles)
                    {
                        try
                        {
                            await _fileIOHelper.DeleteFileAsync(processedFile);
                            _logger.LogInformation("Cleaned up processed file: {FilePath}", processedFile);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, "Failed to clean up processed file: {FilePath}", processedFile);
                        }
                    }

                    foreach (var attachmentFile in parameter.AttachmentFileKeys)
                    {
                        await _fileIOHelper.DeleteFileAsync(attachmentFile);
                    }

                        _logger.LogError(ex, "Error occurred while Updating DMCA Accusation for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Update DMCA Accusation failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Update DMCA Accusation failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ValidateDMCAAccusationReportAsync(ValidateDMCAAccusationReportParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var dmcaAccusationReport = await _dmcaAccusationConclusionReportGenericRepository.FindByIdAsync(parameter.DMCAAccusationConclusionReportId);

                    if(dmcaAccusationReport == null)
                    {
                        throw new Exception($"DMCA Accusation Report with Id: {parameter.DMCAAccusationConclusionReportId} not found");
                    }
                    if(dmcaAccusationReport.IsRejected != null || dmcaAccusationReport.CompletedAt != null)
                    {
                        throw new Exception($"DMCA Accusation Report with Id: {parameter.DMCAAccusationConclusionReportId} has been already processed");
                    }
                    var dmcaAccusation =  await _dmcaAccusationGenericRepository.FindByIdAsync(dmcaAccusationReport.DmcaAccusationId,
                        includeFunc: function => function
                        .Include(da => da.Dmcanotices)
                        .Include(da => da.CounterNotices)
                        .Include(da => da.LawsuitProofs));

                    if(dmcaAccusation == null)
                    {
                        throw new Exception($"DMCA Accusation with Id: {dmcaAccusationReport.DmcaAccusationId} not found");
                    }
                    if(dmcaAccusation.ResolvedAt != null)
                    {
                        throw new Exception($"DMCA Accusation with Id: {dmcaAccusationReport.DmcaAccusationId} has been already resolved");
                    }
                    Console.WriteLine("Fetched DMCA Accusation Id: " + dmcaAccusation.Id);

                    PodcastShowDTO? show = null;
                    PodcastEpisodeWithShowDTO? episode = null;
                    var podcasterId = 0;
                    if (dmcaAccusation.PodcastShowId != null)
                    {
                        show = await GetPodcastShow(dmcaAccusation.PodcastShowId.Value);
                        podcasterId = show.PodcasterId;
                    }
                    if (dmcaAccusation.PodcastEpisodeId != null)
                    {
                        episode = await GetPodcastEpisodeWithShow(dmcaAccusation.PodcastEpisodeId.Value);
                        if (episode.PodcastShow != null)
                        {
                            podcasterId = episode.PodcastShow.PodcasterId;
                        }
                        else
                        {
                            var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                            podcasterId = showFromEpisode.PodcasterId;
                        }
                    }
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(podcasterId);

                    if (parameter.IsValid)
                    {
                        dmcaAccusationReport.IsRejected = false;
                        dmcaAccusationReport.CompletedAt = _dateHelper.GetNowByAppTimeZone();
                        dmcaAccusationReport.UpdatedAt = _dateHelper.GetNowByAppTimeZone();

                        //Close DMCA Accusation
                        dmcaAccusation.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                        dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);

                        switch (dmcaAccusationReport.DmcaAccusationConclusionReportTypeId)
                        {
                            case (int)DMCAAccusationConclusionReportTypeEnum.InvalidDMCANotice:
                                //Send Email to Accuser
                                var mailSendingRequestData = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCANoticeInvalid",
                                        ToEmail = dmcaAccusation.AccuserEmail,
                                        MailObject = new DMCANoticeInvalidMailViewModel
                                        {
                                            AccuserEmail = dmcaAccusation.AccuserEmail,
                                            AccuserFullName = dmcaAccusation.AccuserFullName,
                                            InvalidReason = dmcaAccusationReport.InvalidReason,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var mailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: mailSendingRequestData,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(mailSendingFlow);

                                //Switch Status
                                var invalidDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.InvalidDMCANotice,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(invalidDmcaAccusationStatusTracking);

                                //Update DMCA Notice
                                var dmcaNoticeUpdate = dmcaAccusation.Dmcanotices.OrderByDescending(d => d.CreatedAt).First();
                                Console.WriteLine("Fetched DMCA Notice Id: " + dmcaNoticeUpdate.Id);
                                dmcaNoticeUpdate.IsValid = false;
                                dmcaNoticeUpdate.InvalidReason = dmcaAccusationReport.InvalidReason;
                                dmcaNoticeUpdate.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                                dmcaNoticeUpdate.ValidatedBy = dmcaAccusation.AssignedStaff;
                                await _dmcaNoticeService.UpdateDMCANoticeAsync(dmcaNoticeUpdate);

                                break;
                            case (int)DMCAAccusationConclusionReportTypeEnum.InvalidCounterNotice:
                                //Send Email to Accuser
                                var accuserMailSendingRequestData = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCACounterNoticeInvalidToAccuser",
                                        ToEmail = dmcaAccusation.AccuserEmail,
                                        MailObject = new DMCACounterNoticeInvalidToAccuserMailViewModel
                                        {
                                            AccuserEmail = dmcaAccusation.AccuserEmail,
                                            AccuserFullName = dmcaAccusation.AccuserFullName,
                                            InvalidReason = dmcaAccusationReport.InvalidReason,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var accuserMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accuserMailSendingRequestData,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow);

                                //Send Email to Accused
                                var accusedMailSendingRequestData1 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCACounterNoticeInvalidToAccused",
                                        ToEmail = podcaster.Email,
                                        MailObject = new DMCACounterNoticeInvalidToAccusedMailViewModel
                                        {
                                            PodcasterEmail = podcaster.Email,
                                            PodcasterFullName = podcaster.FullName,
                                            PodcastShowName = show != null ? show.Name : null,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            InvalidReason = dmcaAccusationReport.InvalidReason,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var accusedMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accusedMailSendingRequestData1,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow1);

                                //Punish account
                                var accountPunishRequestData1 = new JObject
                                {
                                    { "AccountId", podcaster.Id },
                                    { "ViolationPoint", 10 }
                                };
                                var accountPunishMessageName1 = "user-violation-punishment-flow";
                                var sagaAccountPunishStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.UserManagementDomain,
                                    requestData: accountPunishRequestData1,
                                    sagaInstanceId: null,
                                    messageName: accountPunishMessageName1);
                                await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage1);
                                _logger.LogInformation("Started user violation punishment flow for Account Id: {AccountId}", podcaster.Id);

                                //Remove Content
                                if (show != null)
                                {
                                    var removeShowRequestData1 = new JObject
                                    {
                                        { "PodcastShowId", show.Id }
                                    };
                                    var removeShowMessageName1 = "dmca-remove-show-flow";
                                    var sagaRemoveShowStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.ContentManagementDomain,
                                        requestData: removeShowRequestData1,
                                        sagaInstanceId: null,
                                        messageName: removeShowMessageName1);
                                    await _messagingService.SendSagaMessageAsync(sagaRemoveShowStartSagaTriggerMessage1);
                                    _logger.LogInformation("Started dmca remove show flow for Podcast Show Id: {PodcastShowId}", show.Id);
                                }
                                if (episode != null)
                                {
                                    var removeEpisodeEpisodeRequestData1 = new JObject
                                    {
                                        { "PodcastEpisodeId", episode.Id }
                                    };
                                    var removeEpisodeMessageName1 = "dmca-remove-episode-flow";
                                    var sagaRemoveEpisodeStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.ContentManagementDomain,
                                        requestData: removeEpisodeEpisodeRequestData1,
                                        sagaInstanceId: null,
                                        messageName: removeEpisodeMessageName1);
                                    await _messagingService.SendSagaMessageAsync(sagaRemoveEpisodeStartSagaTriggerMessage1);
                                    _logger.LogInformation("Started dmca remove episode flow for Podcast Episode Id: {PodcastEpisodeId}", episode.Id);
                                }

                                //Second Level Punishment
                                var accountPunishRequestData2 = new JObject
                                {
                                    { "AccountId", podcaster.Id },
                                    { "ViolationPoint", 200 }
                                };
                                var accountPunishMessageName2 = "user-violation-punishment-flow";
                                var sagaAccountPunishStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.UserManagementDomain,
                                    requestData: accountPunishRequestData2,
                                    sagaInstanceId: null,
                                    messageName: accountPunishMessageName2);
                                await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage2);
                                _logger.LogInformation("Started user violation punishment flow for Account Id: {AccountId}", podcaster.Id);

                                //Switch Status
                                var invalidCounterNoticeDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.InvalidCounterNotice,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(invalidCounterNoticeDmcaAccusationStatusTracking);

                                //Update Counter Notice
                                var counterNoticeUpdate = dmcaAccusation.CounterNotices.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                                counterNoticeUpdate.IsValid = false;
                                counterNoticeUpdate.InvalidReason = dmcaAccusationReport.InvalidReason;
                                counterNoticeUpdate.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                                counterNoticeUpdate.ValidatedBy = dmcaAccusation.AssignedStaff;
                                await _counterNoticeService.UpdateCounterNotice(counterNoticeUpdate);

                                break;
                            case (int)DMCAAccusationConclusionReportTypeEnum.InvalidLawsuitProof:
                                //Restore Content
                                var restoreContentRequestData1 = new JObject
                                {
                                    { "PodcastShowId", show != null ? show.Id : null },
                                    { "PodcastEpisodeId", episode != null ? episode.Id : null }
                                };
                                var restoreContentMessageName1 = "content-restoration-flow";
                                var sagaRestoreContentStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ContentManagementDomain,
                                    requestData: restoreContentRequestData1,
                                    sagaInstanceId: null,
                                    messageName: restoreContentMessageName1);
                                await _messagingService.SendSagaMessageAsync(sagaRestoreContentStartSagaTriggerMessage1);
                                _logger.LogInformation("Sent content restoration message for DMCA Accusation Id: {DMCAAccusationId}", dmcaAccusation.Id);

                                //Send Email to Accused
                                var accusedMailSendingRequestData2 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCALawsuitProofInvalidToAccused",
                                        ToEmail = podcaster.Email,
                                        MailObject = new DMCALawsuitProofInvalidToAccusedMailViewModel
                                        {
                                            PodcasterEmail = podcaster.Email,
                                            PodcasterFullName = podcaster.FullName,
                                            PodcastShowName = show != null ? show.Name : null,
                                            PodcastEpisodeName = episode != null ? episode.Name : null                                        }
                                    }
                                });
                                var accusedMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accusedMailSendingRequestData2,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow2);

                                //Send Email to Accuser
                                var accuserMailSendingRequestData2 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCALawsuitProofInvalidToAccuser",
                                        ToEmail = dmcaAccusation.AccuserEmail,
                                        MailObject = new DMCALawsuitProofInvalidToAccuserMailViewModel
                                        {
                                            AccuserEmail = dmcaAccusation.AccuserEmail,
                                            AccuserFullName = dmcaAccusation.AccuserFullName,
                                            PodcastShowName = show != null ? show.Name : null,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            InvalidReason = dmcaAccusationReport.InvalidReason,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var accuserMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accuserMailSendingRequestData2,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow2);

                                //Switch Status
                                var invalidLawsuitProofDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.InvalidLawsuitProof,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(invalidLawsuitProofDmcaAccusationStatusTracking);

                                //Update Lawsuit Proof
                                var lawsuitProofUpdate = dmcaAccusation.LawsuitProofs.OrderByDescending(d => d.CreatedAt).FirstOrDefault();
                                lawsuitProofUpdate.IsValid = false;
                                lawsuitProofUpdate.InValidReason = dmcaAccusationReport.InvalidReason;
                                lawsuitProofUpdate.ValidatedAt = _dateHelper.GetNowByAppTimeZone();
                                lawsuitProofUpdate.ValidatedBy = dmcaAccusation.AssignedStaff;  
                                await _lawsuitProofService.UpdateLawsuitProof(lawsuitProofUpdate);

                                break;
                            case (int)DMCAAccusationConclusionReportTypeEnum.AccuserLawsuitWin:
                                //Send Email to Accuser
                                var accuserMailSendingRequestData3 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCALawsuitProofAccuserWinToAccuser",
                                        ToEmail = dmcaAccusation.AccuserEmail,
                                        MailObject = new DMCALawsuitProofAccuserWinToAccuserMailViewModel
                                        {
                                            AccuserEmail = dmcaAccusation.AccuserEmail,
                                            AccuserFullName = dmcaAccusation.AccuserFullName,
                                            PodcastShowName = show != null ? show.Name : null,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var accuserMailSendingFlow3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accuserMailSendingRequestData3,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow3);

                                //Send Email to Accused
                                var accusedMailSendingRequestData3 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCALawsuitProofPodcasterWinToAccused",
                                        ToEmail = podcaster.Email,
                                        MailObject = new DMCALawsuitProofAccuserWinToAccusedMailViewModel
                                        {
                                            PodcasterEmail = podcaster.Email,
                                            PodcasterFullName = podcaster.FullName,
                                            PodcastShowName = show != null ? show.Name : null,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var accusedMailSendingFlow3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accusedMailSendingRequestData3,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow3);

                                //Remove Content
                                if (show != null)
                                {
                                    var removeShowRequestData2 = new JObject
                                    {
                                        { "PodcastShowId", show.Id }
                                    };
                                    var removeShowMessageName2 = "dmca-remove-show-flow";
                                    var sagaRemoveShowStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.ContentManagementDomain,
                                        requestData: removeShowRequestData2,
                                        sagaInstanceId: null,
                                        messageName: removeShowMessageName2);
                                    await _messagingService.SendSagaMessageAsync(sagaRemoveShowStartSagaTriggerMessage2);
                                    _logger.LogInformation("Sent remove show message for ShowId: {ShowId}", show.Id);
                                }
                                if (episode != null)
                                {
                                    var removeEpisodeEpisodeRequestData2 = new JObject
                                    {
                                        { "PodcastEpisodeId", episode.Id }
                                    };
                                    var removeEpisodeMessageName2 = "dmca-remove-episode-flow";
                                    var sagaRemoveEpisodeStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.ContentManagementDomain,
                                        requestData: removeEpisodeEpisodeRequestData2,
                                        sagaInstanceId: null,
                                        messageName: removeEpisodeMessageName2);
                                    await _messagingService.SendSagaMessageAsync(sagaRemoveEpisodeStartSagaTriggerMessage2);
                                    _logger.LogInformation("Sent remove episode message for EpisodeId: {EpisodeId}", episode.Id);
                                }

                                //Punish account
                                var accountPunishRequestData3 = new JObject
                                {
                                    { "AccountId", podcaster.Id },
                                    { "ViolationPoint", 500 }
                                };
                                var accountPunishMessageName3 = "user-violation-punishment-flow";
                                var sagaAccountPunishStartSagaTriggerMessage3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.UserManagementDomain,
                                    requestData: accountPunishRequestData3,
                                    sagaInstanceId: null,
                                    messageName: accountPunishMessageName3);
                                await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage3);
                                _logger.LogInformation("Sent account punishment message for AccountId: {AccountId}", podcaster.Id);

                                //Switch Status
                                var accuserWinDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.AccuserLawsuitWin,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(accuserWinDmcaAccusationStatusTracking);
                                break;
                            case (int)DMCAAccusationConclusionReportTypeEnum.PodcasterLawsuitWin:
                                //Restore Content
                                var restoreContentRequestData2 = new JObject
                                {
                                    { "PodcastShowId", show != null ? show.Id : null },
                                    { "PodcastEpisodeId", episode != null ? episode.Id : null }
                                };
                                var restoreContentMessageName2 = "content-restoration-flow";
                                var sagaRestoreContentStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ContentManagementDomain,
                                    requestData: restoreContentRequestData2,
                                    sagaInstanceId: null,
                                    messageName: restoreContentMessageName2);
                                await _messagingService.SendSagaMessageAsync(sagaRestoreContentStartSagaTriggerMessage2, null);
                                _logger.LogInformation("Sent content restoration message for DMCA Accusation Id: {DMCAAccusationId}", dmcaAccusation.Id);

                                //Send Email to Accuser
                                var accuserMailSendingRequestData4 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCALawsuitProofPodcasterWinToAccuser",
                                        ToEmail = dmcaAccusation.AccuserEmail,
                                        MailObject = new DMCALawsuitProofPodcasterWinToAccuserMailViewModel
                                        {
                                            AccuserEmail = dmcaAccusation.AccuserEmail,
                                            AccuserFullName = dmcaAccusation.AccuserFullName,
                                            PodcastShowName = show != null ? show.Name : null,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var accuserMailSendingFlow4 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accuserMailSendingRequestData4,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow4);

                                //Send Email to Accused
                                var accusedMailSendingRequestData4 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCALawsuitProofPodcasterWinToAccused",
                                        ToEmail = podcaster.Email,
                                        MailObject = new DMCALawsuitProofPodcasterWinToAccusedMailViewModel
                                        {
                                            PodcasterEmail = podcaster.Email,
                                            PodcasterFullName = podcaster.FullName,
                                            PodcastShowName = show != null ? show.Name : null,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            CompletedAt = dmcaAccusationReport.CompletedAt.Value
                                        }
                                    }
                                });
                                var accusedMailSendingFlow4 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accusedMailSendingRequestData4,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow4);

                                //Switch Status
                                var podcasterWinDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.PodcasterLawsuitWin,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(podcasterWinDmcaAccusationStatusTracking);
                                break;
                            default:
                                throw new Exception("DMCA Accusation Conclusion Report Type not recognize");
                        }
                    } else
                    {
                        dmcaAccusationReport.IsRejected = true;
                        dmcaAccusationReport.CompletedAt = _dateHelper.GetNowByAppTimeZone();
                        dmcaAccusationReport.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    }
                    await _dmcaAccusationConclusionReportGenericRepository.UpdateAsync(dmcaAccusationReport.Id, dmcaAccusationReport);

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                        {
                            { "DMCAAccusationReportId", dmcaAccusationReport.Id },
                            { "IsValid", parameter.IsValid },
                            { "UpdatedAt", dmcaAccusationReport.UpdatedAt }
                        };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully validate DMCA Accusation report for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while validating DMCA Accusation report for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Validating DMCA Accusation report failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Validating DMCA Accusation report failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelDMCAAccusationReportAsync(CancelDMCAAccusationReportParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var dmcaAccusationConclusionReport = await _dmcaAccusationConclusionReportGenericRepository.FindByIdAsync(parameter.DMCAAccusationConclusionReportId);

                    if(dmcaAccusationConclusionReport == null)
                    {
                        throw new Exception($"DMCA Report with id {dmcaAccusationConclusionReport.Id} not found");
                    }
                    if(dmcaAccusationConclusionReport.IsRejected != null)
                    {
                        throw new Exception($"DMCA Report with id {dmcaAccusationConclusionReport.Id} has been processed and cannot be cancelled");
                    }
                    if(dmcaAccusationConclusionReport.CompletedAt != null)
                    {
                        throw new Exception($"DMCA Report with id {dmcaAccusationConclusionReport.Id} has been completed and cannot be cancelled");
                    }
                    dmcaAccusationConclusionReport.CancelledAt = _dateHelper.GetNowByAppTimeZone();
                    await _dmcaAccusationConclusionReportGenericRepository.UpdateAsync(dmcaAccusationConclusionReport.Id, dmcaAccusationConclusionReport);

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    newResponseData["CancelledAt"] = dmcaAccusationConclusionReport.CancelledAt;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully cancel DMCA Accusation report for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while cancelling DMCA Accusation report for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancelling DMCA Accusation report failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancelling DMCA Accusation report failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissOtherEpisodeDMCADMCARemoveEpisodeForcedAsync(DismissOtherEpisodeDMCADMCARemoveEpisodeForcedParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var reason = DMCAAccusationDismissReasonEnum.EpisodeRemovedInDMCAAccusation;
                    string description = reason.GetDescription();

                    var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da => da.PodcastEpisodeId == parameter.PodcastEpisodeId &&
                        da.ResolvedAt == null)
                        .ToListAsync();

                    if (dmcaList.Count != 0)
                    {
                        //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                        //    .Where(da => da.PodcastEpisodeId == parameter.PodcastEpisodeId
                        //    && da.ResolvedAt != null)
                        //    .ToListAsync();

                        //if (dmcaAccusationExists.Count == 0)
                        //{
                        //    var episode = await GetPodcastEpisode(parameter.PodcastEpisodeId);
                        //    var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                        //    var podcasterId = showFromEpisode.PodcasterId;

                        //    //Punish account
                        //    var accountPunishRequestData = new JObject
                        //    {
                        //        { "AccountId", podcasterId },
                        //        { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                        //    };
                        //    var accountPunishMessageName = "user-violation-punishment-flow";
                        //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        //        topic: KafkaTopicEnum.ContentManagementDomain,
                        //        requestData: accountPunishRequestData,
                        //        sagaInstanceId: null,
                        //        messageName: accountPunishMessageName);
                        //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                        //}
                    
                        foreach (var dmca in dmcaList)
                        {
                            var currentDmcaStatus = dmca.DmcaaccusationStatusTrackings
                                .OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault().DmcaAccusationStatusId;
                            
                            var dismissedStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmca.Id,
                                DmcaAccusationStatusId = currentDmcaStatus == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                            dmca.DismissReason = description;
                            dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss other episode DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while dismissing other episode DMCA Accusation for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismissing other episode DMCA Accusation failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismissing other episode DMCA Accusation failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        //public async Task DMCAAccusationResponseTimeAllowedChecking()
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var dmcaNoticeResponseTime = _dmcaAccusationConfig.DMCANoticeResponseTime;
        //            var dmcaCounterNoticeResponseTime = _dmcaAccusationConfig.DMCACounterNoticeResponseTime;

        //            var dmcaAccusationCheckingList = await _dmcaAccusationGenericRepository.FindAll(
        //                includeFunc: function => function
        //                .Include(da => da.Dmcanotices)
        //                .Include(da => da.CounterNotices)
        //                .Include(da => da.LawsuitProofs)
        //                .Include(da => da.DmcaaccusationStatusTrackings))
        //                .Where(da => 
        //                da.DmcaaccusationStatusTrackings.OrderByDescending(st => st.CreatedAt).FirstOrDefault().DmcaAccusationStatusId == (int)DMCAAccusationStatusEnum.ValidDMCANotice && da.Dmcanotices.OrderByDescending(dn => dn.CreatedAt).FirstOrDefault().ValidatedAt.Value.AddDays(dmcaNoticeResponseTime) < _dateHelper.GetNowByAppTimeZone() ||
        //                da.DmcaaccusationStatusTrackings.OrderByDescending(st => st.CreatedAt).FirstOrDefault().DmcaAccusationStatusId == (int)DMCAAccusationStatusEnum.ValidCounterNotice && da.CounterNotices.OrderByDescending(dn => dn.CreatedAt).FirstOrDefault().ValidatedAt.Value.AddDays(dmcaCounterNoticeResponseTime) < _dateHelper.GetNowByAppTimeZone()
        //                )
        //                .ToListAsync();
        //            foreach(var dmcaAccusation in dmcaAccusationCheckingList)
        //            {
        //                PodcastShowDTO? show = null;
        //                PodcastEpisodeDTO? episode = null;
        //                var podcasterId = 0;
        //                if (dmcaAccusation.PodcastShowId != null)
        //                {
        //                    show = await GetPodcastShow(dmcaAccusation.PodcastShowId.Value);
        //                    podcasterId = show.PodcasterId;
        //                }
        //                if (dmcaAccusation.PodcastEpisodeId != null)
        //                {
        //                    episode = await GetPodcastEpisode(dmcaAccusation.PodcastEpisodeId.Value);
        //                    var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
        //                    podcasterId = showFromEpisode.PodcasterId;
        //                }
        //                var podcaster = await _accountCachingService.GetAccountStatusCacheById(podcasterId);

        //                var currentStatusId = dmcaAccusation.DmcaaccusationStatusTrackings.OrderByDescending(st => st.CreatedAt).FirstOrDefault().DmcaAccusationStatusId;
        //                if(currentStatusId == (int)DMCAAccusationStatusEnum.ValidDMCANotice)
        //                {
        //                    //Send Email to Accused
        //                    var accusedMailSendingRequestData1 = JObject.FromObject(new
        //                    {
        //                        SendModerationServiceEmailInfo = new
        //                        {
        //                            MailTypeName = "DMCANoticeValidNotResponseInTimeToAccused",
        //                            ToEmail = podcaster.Email,
        //                            MailObject = new DMCANoticeValidNotResponseInTimeToAccusedMailViewModel
        //                            {
        //                                PodcasterEmail = podcaster.Email,
        //                                PodcasterFullName = podcaster.FullName,
        //                                PodcastEpisodeName = episode != null ? episode.Name : null,
        //                                PodcastShowName = show != null ? show.Name : null
        //                            }
        //                        }
        //                    });
        //                    var accusedMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.ReportManagementDomain,
        //                        requestData: accusedMailSendingRequestData1,
        //                        sagaInstanceId: null,
        //                        messageName: "moderation-service-mail-sending-flow");
        //                    await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow1);

        //                    //Send Email to Accuser
        //                    var accuserMailSendingRequestData1 = JObject.FromObject(new
        //                    {
        //                        SendModerationServiceEmailInfo = new
        //                        {
        //                            MailTypeName = "DMCANoticeValidNotResponseInTimeToAccuser",
        //                            ToEmail = dmcaAccusation.AccuserEmail,
        //                            MailObject = new DMCANoticeValidNotResponseInTimeToAccuserMailViewModel
        //                            {
        //                                AccuserEmail = dmcaAccusation.AccuserEmail,
        //                                AccuserFullName = dmcaAccusation.AccuserFullName,
        //                                PodcastEpisodeName = episode != null ? episode.Name : null,
        //                                PodcastShowName = show != null ? show.Name : null
        //                            }
        //                        }
        //                    });
        //                    var accuserMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.ReportManagementDomain,
        //                        requestData: accuserMailSendingRequestData1,
        //                        sagaInstanceId: null,
        //                        messageName: "moderation-service-mail-sending-flow");
        //                    await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow1);

        //                    //Remove Content
        //                    if (show != null)
        //                    {
        //                        var removeShowRequestData1 = new JObject
        //                            {
        //                                { "PodcastShowId", show.Id }
        //                            };
        //                        var removeShowMessageName1 = "dmca-remove-show-flow";
        //                        var sagaRemoveShowStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                            topic: KafkaTopicEnum.ContentManagementDomain,
        //                            requestData: removeShowRequestData1,
        //                            sagaInstanceId: null,
        //                            messageName: removeShowMessageName1);
        //                        await _messagingService.SendSagaMessageAsync(sagaRemoveShowStartSagaTriggerMessage1);
        //                    }
        //                    if (episode != null)
        //                    {
        //                        var removeEpisodeEpisodeRequestData1 = new JObject
        //                            {
        //                                { "PodcastEpisodeId", episode.Id }
        //                            };
        //                        var removeEpisodeMessageName1 = "dmca-remove-episode-flow";
        //                        var sagaRemoveEpisodeStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                            topic: KafkaTopicEnum.ContentManagementDomain,
        //                            requestData: removeEpisodeEpisodeRequestData1,
        //                            sagaInstanceId: null,
        //                            messageName: removeEpisodeMessageName1);
        //                        await _messagingService.SendSagaMessageAsync(sagaRemoveEpisodeStartSagaTriggerMessage1);
        //                    }
        //                }
        //                else if(currentStatusId == (int)DMCAAccusationStatusEnum.ValidCounterNotice)
        //                {
        //                    //Restore Content
        //                    var restoreContentRequestData1 = new JObject
        //                        {
        //                            { "PodcastShowId", show != null ? show.Id : null },
        //                            { "PodcastEpisodeId", episode != null ? episode.Id : null }
        //                        };
        //                    var restoreContentMessageName1 = "content-restoration-flow";
        //                    var sagaRestoreContentStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.ContentManagementDomain,
        //                        requestData: restoreContentRequestData1,
        //                        sagaInstanceId: null,
        //                        messageName: restoreContentMessageName1);
        //                    await _messagingService.SendSagaMessageAsync(sagaRestoreContentStartSagaTriggerMessage1);

        //                    //Send Email to Accused
        //                    var accusedMailSendingRequestData2 = JObject.FromObject(new
        //                    {
        //                        SendModerationServiceEmailInfo = new
        //                        {
        //                            MailTypeName = "DMCACounterNoticeValidNotResponseInTimeToAccused",
        //                            ToEmail = podcaster.Email,
        //                            MailObject = new DMCACounterNoticeValidNotResponseInTimeToAccusedMailViewModel
        //                            {
        //                                PodcasterEmail = podcaster.Email,
        //                                PodcasterFullName = podcaster.FullName
        //                            }
        //                        }
        //                    });
        //                    var accusedMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.ReportManagementDomain,
        //                        requestData: accusedMailSendingRequestData2,
        //                        sagaInstanceId: null,
        //                        messageName: "moderation-service-mail-sending-flow");
        //                    await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow2);

        //                    //Send Email to Accuser
        //                    var accuserMailSendingRequestData2 = JObject.FromObject(new
        //                    {
        //                        SendModerationServiceEmailInfo = new
        //                        {
        //                            MailTypeName = "DMCACounterNoticeValidNotResponseInTimeToAccuser",
        //                            ToEmail = dmcaAccusation.AccuserEmail,
        //                            MailObject = new DMCACounterNoticeValidNotResponseInTimeToAccuserMailViewModel
        //                            {
        //                                AccuserEmail = dmcaAccusation.AccuserEmail,
        //                                AccuserFullName = dmcaAccusation.AccuserFullName
        //                            }
        //                        }
        //                    });
        //                    var accuserMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.ReportManagementDomain,
        //                        requestData: accuserMailSendingRequestData2,
        //                        sagaInstanceId: null,
        //                        messageName: "moderation-service-mail-sending-flow");
        //                    await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow2);
        //                }

        //                //Close DMCA Accusation
        //                dmcaAccusation.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
        //                dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
        //                await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);

        //            }

        //            await transaction.CommitAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while checking DMCA Accusation response time allowed");
        //            throw new HttpRequestException("Error occurred while checking DMCA Accusation response time allowed. Error: " + ex.Message);
        //        }
        //    }
        //}
        public async Task DMCANoticeResponseTimeoutAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var dmcaNoticeResponseTime = _dmcaAccusationConfig.DMCANoticeResponseTime;
                    //var dmcaCounterNoticeResponseTime = _dmcaAccusationConfig.DMCACounterNoticeResponseTime;
                    //var dmcaNoticeResponseTime = 0;

                    var dmcaAccusationCheckingList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.Dmcanotices)
                        .Include(da => da.CounterNotices)
                        .Include(da => da.LawsuitProofs)
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da =>
                        da.DmcaaccusationStatusTrackings.OrderByDescending(st => st.CreatedAt).FirstOrDefault().DmcaAccusationStatusId == (int)DMCAAccusationStatusEnum.ValidDMCANotice && da.Dmcanotices.OrderByDescending(dn => dn.CreatedAt).FirstOrDefault().ValidatedAt.Value.AddDays(dmcaNoticeResponseTime) < _dateHelper.GetNowByAppTimeZone()
                        && da.ResolvedAt == null)
                        .ToListAsync();

                    if(dmcaAccusationCheckingList != null)
                    {
                        foreach (var dmcaAccusation in dmcaAccusationCheckingList)
                        {
                            PodcastShowDTO? show = null;
                            PodcastEpisodeWithShowDTO? episode = null;
                            var podcasterId = 0;
                            if (dmcaAccusation.PodcastShowId != null)
                            {
                                show = await GetPodcastShow(dmcaAccusation.PodcastShowId.Value);
                                if(show == null)
                                {
                                    _logger.LogWarning("Podcast Show is null for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                    continue;
                                }
                                podcasterId = show.PodcasterId;
                            }
                            if (dmcaAccusation.PodcastEpisodeId != null)
                            {
                                episode = await GetPodcastEpisodeWithShow(dmcaAccusation.PodcastEpisodeId.Value);
                                if(episode == null)
                                {
                                    _logger.LogWarning("Podcast Episode is null for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                    continue;
                                }
                                if (episode.PodcastShow != null)
                                {
                                    podcasterId = episode.PodcastShow.PodcasterId;
                                }
                                else
                                {
                                    var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                                    if(showFromEpisode == null)
                                    {
                                        _logger.LogWarning("Podcast Show from Episode is null for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                        continue;
                                    }
                                    podcasterId = showFromEpisode.PodcasterId;
                                }
                            }
                            if(podcasterId == 0)
                            {
                                _logger.LogWarning("PodcasterId is 0 for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                continue;
                            }
                            var podcaster = await _accountCachingService.GetAccountStatusCacheById(podcasterId);

                            var currentStatusId = dmcaAccusation.DmcaaccusationStatusTrackings.OrderByDescending(st => st.CreatedAt).FirstOrDefault().DmcaAccusationStatusId;
                            if (currentStatusId == (int)DMCAAccusationStatusEnum.ValidDMCANotice)
                            {
                                //Send Email to Accused
                                var accusedMailSendingRequestData1 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCANoticeValidNotResponseInTimeToAccused",
                                        ToEmail = podcaster.Email,
                                        MailObject = new DMCANoticeValidNotResponseInTimeToAccusedMailViewModel
                                        {
                                            PodcasterEmail = podcaster.Email,
                                            PodcasterFullName = podcaster.FullName,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            PodcastShowName = show != null ? show.Name : null
                                        }
                                    }
                                });
                                var accusedMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accusedMailSendingRequestData1,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow1);

                                //Send Email to Accuser
                                var accuserMailSendingRequestData1 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCANoticeValidNotResponseInTimeToAccuser",
                                        ToEmail = dmcaAccusation.AccuserEmail,
                                        MailObject = new DMCANoticeValidNotResponseInTimeToAccuserMailViewModel
                                        {
                                            AccuserEmail = dmcaAccusation.AccuserEmail,
                                            AccuserFullName = dmcaAccusation.AccuserFullName,
                                            PodcastEpisodeName = episode != null ? episode.Name : null,
                                            PodcastShowName = show != null ? show.Name : null
                                        }
                                    }
                                });
                                var accuserMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accuserMailSendingRequestData1,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow1);

                                //Remove Content
                                if (show != null)
                                {
                                    var removeShowRequestData1 = new JObject
                                        {
                                            { "PodcastShowId", show.Id }
                                        };
                                    var removeShowMessageName1 = "dmca-remove-show-flow";
                                    var sagaRemoveShowStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.ContentManagementDomain,
                                        requestData: removeShowRequestData1,
                                        sagaInstanceId: null,
                                        messageName: removeShowMessageName1);
                                    await _messagingService.SendSagaMessageAsync(sagaRemoveShowStartSagaTriggerMessage1);
                                }
                                if (episode != null)
                                {
                                    var removeEpisodeEpisodeRequestData1 = new JObject
                                        {
                                            { "PodcastEpisodeId", episode.Id }
                                        };
                                    var removeEpisodeMessageName1 = "dmca-remove-episode-flow";
                                    var sagaRemoveEpisodeStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.ContentManagementDomain,
                                        requestData: removeEpisodeEpisodeRequestData1,
                                        sagaInstanceId: null,
                                        messageName: removeEpisodeMessageName1);
                                    await _messagingService.SendSagaMessageAsync(sagaRemoveEpisodeStartSagaTriggerMessage1);
                                }
                            }

                            //Close DMCA Accusation
                            dmcaAccusation.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while checking DMCA Notice response time allowed");
                    throw new HttpRequestException("Error occurred while checking DMCA Notice response time allowed. Error: " + ex.Message);
                }
            }
        }
        public async Task CounterNoticeResponseTimeoutAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //var dmcaNoticeResponseTime = _dmcaAccusationConfig.DMCANoticeResponseTime;
                    var dmcaCounterNoticeResponseTime = _dmcaAccusationConfig.DMCACounterNoticeResponseTime;
                    //var dmcaCounterNoticeResponseTime = 0;

                    var dmcaAccusationCheckingList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.Dmcanotices)
                        .Include(da => da.CounterNotices)
                        .Include(da => da.LawsuitProofs)
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da =>
                        da.DmcaaccusationStatusTrackings.OrderByDescending(st => st.CreatedAt).FirstOrDefault().DmcaAccusationStatusId == (int)DMCAAccusationStatusEnum.ValidCounterNotice && da.CounterNotices.OrderByDescending(dn => dn.CreatedAt).FirstOrDefault().ValidatedAt.Value.AddDays(dmcaCounterNoticeResponseTime) < _dateHelper.GetNowByAppTimeZone()
                        && da.ResolvedAt == null)
                        .ToListAsync();

                    if(dmcaAccusationCheckingList != null)
                    {
                        foreach (var dmcaAccusation in dmcaAccusationCheckingList)
                        {
                            PodcastShowDTO? show = null;
                            PodcastEpisodeWithShowDTO? episode = null;
                            var podcasterId = 0;
                            if (dmcaAccusation.PodcastShowId != null)
                            {
                                show = await GetPodcastShow(dmcaAccusation.PodcastShowId.Value);
                                if(show == null)
                                {
                                    _logger.LogWarning("Podcast Show is null for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                    continue;
                                }
                                podcasterId = show.PodcasterId;
                            }
                            if (dmcaAccusation.PodcastEpisodeId != null)
                            {
                                episode = await GetPodcastEpisodeWithShow(dmcaAccusation.PodcastEpisodeId.Value);
                                if(episode == null)
                                {
                                    _logger.LogWarning("Podcast Episode is null for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                    continue;
                                }
                                if (episode.PodcastShow != null)
                                {
                                    podcasterId = episode.PodcastShow.PodcasterId;
                                }
                                else
                                {
                                    var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                                    if(showFromEpisode == null)
                                    {
                                        _logger.LogWarning("Podcast Show from Episode is null for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                        continue;
                                    }
                                    podcasterId = showFromEpisode.PodcasterId;
                                }
                            }
                            if(podcasterId == 0)
                            {
                                _logger.LogWarning("Unable to get PodcasterId for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusation.Id);
                                continue;
                            }
                            var podcaster = await _accountCachingService.GetAccountStatusCacheById(podcasterId);

                            var currentStatusId = dmcaAccusation.DmcaaccusationStatusTrackings.OrderByDescending(st => st.CreatedAt).FirstOrDefault().DmcaAccusationStatusId;
                            if (currentStatusId == (int)DMCAAccusationStatusEnum.ValidCounterNotice)
                            {
                                //Restore Content
                                var restoreContentRequestData1 = new JObject
                                    {
                                        { "PodcastShowId", show != null ? show.Id : null },
                                        { "PodcastEpisodeId", episode != null ? episode.Id : null }
                                    };
                                var restoreContentMessageName1 = "content-restoration-flow";
                                var sagaRestoreContentStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ContentManagementDomain,
                                    requestData: restoreContentRequestData1,
                                    sagaInstanceId: null,
                                    messageName: restoreContentMessageName1);
                                await _messagingService.SendSagaMessageAsync(sagaRestoreContentStartSagaTriggerMessage1);

                                //Send Email to Accused
                                var accusedMailSendingRequestData2 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCACounterNoticeValidNotResponseInTimeToAccused",
                                        ToEmail = podcaster.Email,
                                        MailObject = new DMCACounterNoticeValidNotResponseInTimeToAccusedMailViewModel
                                        {
                                            PodcasterEmail = podcaster.Email,
                                            PodcasterFullName = podcaster.FullName
                                        }
                                    }
                                });
                                var accusedMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accusedMailSendingRequestData2,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accusedMailSendingFlow2);

                                //Send Email to Accuser
                                var accuserMailSendingRequestData2 = JObject.FromObject(new
                                {
                                    SendModerationServiceEmailInfo = new
                                    {
                                        MailTypeName = "DMCACounterNoticeValidNotResponseInTimeToAccuser",
                                        ToEmail = dmcaAccusation.AccuserEmail,
                                        MailObject = new DMCACounterNoticeValidNotResponseInTimeToAccuserMailViewModel
                                        {
                                            AccuserEmail = dmcaAccusation.AccuserEmail,
                                            AccuserFullName = dmcaAccusation.AccuserFullName
                                        }
                                    }
                                });
                                var accuserMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ReportManagementDomain,
                                    requestData: accuserMailSendingRequestData2,
                                    sagaInstanceId: null,
                                    messageName: "moderation-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(accuserMailSendingFlow2);
                            }

                            //Close DMCA Accusation
                            dmcaAccusation.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);

                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while checking Counter Notice response time allowed");
                    throw new HttpRequestException("Error occurred while checking Counter Notice response time allowed. Error: " + ex.Message);
                }
            }
        }
        public async Task DismissEpisodeDMCAUnpublishEpisodeForceAsync(DismissEpisodeDMCAUnpublishEpisodeForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da => da.PodcastEpisodeId == parameter.PodcastEpisodeId
                        && da.ResolvedAt == null)
                        .ToListAsync();

                    Guid? DmcaDismissedEpisodeId = null;
                    if (dmcaList.Count != 0)
                    {
                        //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                        //    .Where(da => da.PodcastEpisodeId == parameter.PodcastEpisodeId
                        //    && da.ResolvedAt != null)
                        //    .ToListAsync();

                        var reason = DMCAAccusationDismissReasonEnum.EpisodeUnpublished;
                        string description = reason.GetDescription();

                        //if (dmcaAccusationExists.Count == 0)
                        //{
                        
                        //}

                        var check = false;
                        foreach (var dmca in dmcaList)
                        {
                            var currentDmcaStatus = dmca.DmcaaccusationStatusTrackings
                                .OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault().DmcaAccusationStatusId;

                            if(currentDmcaStatus != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                            {
                                if (!check)
                                {
                                    DmcaDismissedEpisodeId = parameter.PodcastEpisodeId;

                                    var episode = await GetPodcastEpisodeWithShow(parameter.PodcastEpisodeId);
                                    if(episode == null)
                                    {
                                        throw new HttpRequestException("Podcast Episode not found for Podcast Episode Id: " + parameter.PodcastEpisodeId);
                                    }
                                    var podcasterId = 0;
                                    if(episode.PodcastShow != null)
                                    {
                                        podcasterId = episode.PodcastShow.PodcasterId;
                                    }
                                    else
                                    {
                                        var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                                        if(showFromEpisode == null)
                                        {
                                            throw new HttpRequestException("Podcast Show not found for Podcast Show Id: " + episode.PodcastShowId);
                                        }
                                        podcasterId = showFromEpisode.PodcasterId;
                                    }

                                    if(podcasterId != 0)
                                    {
                                        //Punish account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                    }
                                    else
                                    {
                                        //_logger.LogWarning("PodcasterId is 0 for Podcast Episode Id: {PodcastEpisodeId}", parameter.PodcastEpisodeId);
                                        throw new HttpRequestException("Unable to get PodcasterId for Podcast Episode Id: " + parameter.PodcastEpisodeId);
                                    }
                                    check = true;
                                }
                            }

                            var dismissedStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmca.Id,
                                DmcaAccusationStatusId = currentDmcaStatus == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                            dmca.DismissReason = description;
                            dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedEpisodeId"] = DmcaDismissedEpisodeId;
                    newRequestData["DmcaDismissedEpisodeId"] = DmcaDismissedEpisodeId;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss other episode DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Episode DMCA Unpublish Episode Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Episode DMCA Unpublish Episode Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Episode DMCA Unpublish Episode Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissOtherShowDMCADMCARemoveShowForceAsync(DismissOtherShowDMCADMCARemoveShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da => da.PodcastShowId == parameter.PodcastShowId
                        && da.ResolvedAt == null)
                        .ToListAsync();

                    if (dmcaList.Count != 0)
                    {

                        //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                        //    .Where(da => da.PodcastShowId == parameter.PodcastShowId
                        //    && da.ResolvedAt != null)
                        //    .ToListAsync();

                        var reason = DMCAAccusationDismissReasonEnum.ShowRemovedInDMCAAccusation;
                        string description = reason.GetDescription();

                        //if (dmcaAccusationExists.Count == 0)
                        //{
                        //    var show = await GetPodcastShow(parameter.PodcastShowId);
                        //    var podcasterId = show.PodcasterId;

                        //    //Punish account
                        //    var accountPunishRequestData = new JObject
                        //{
                        //    { "AccountId", podcasterId },
                        //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                        //};
                        //    var accountPunishMessageName = "user-violation-punishment-flow";
                        //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        //        topic: KafkaTopicEnum.ContentManagementDomain,
                        //        requestData: accountPunishRequestData,
                        //        sagaInstanceId: null,
                        //        messageName: accountPunishMessageName);
                        //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                        //}

                        foreach (var dmca in dmcaList)
                        {
                            var currentDMCAStatus = dmca.DmcaaccusationStatusTrackings
                                .OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault().DmcaAccusationStatusId;
                            var dismissedStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmca.Id,
                                DmcaAccusationStatusId = currentDMCAStatus == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                            dmca.DismissReason = description;
                            dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss other show DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while dismissing other show DMCA Accusation for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismissing other show DMCA Accusation failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismissing other show DMCA Accusation failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissShowEpisodesDMCADMCARemoveShowForceAsync(DismissShowEpisodesDMCADMCARemoveShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var show = await GetPodcastShowWithEpisode(parameter.PodcastShowId);
                    if(show == null)
                    {
                        throw new HttpRequestException("Podcast Show not found for Podcast Show Id: " + parameter.PodcastShowId);
                    }
                    var podcasterId = show.PodcasterId;

                    foreach(var episodeId in show.PodcastEpisodes.Select(s => s.Id))
                    {
                        var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(da => da.DmcaaccusationStatusTrackings))
                            .Where(da => da.PodcastEpisodeId == episodeId
                            && da.ResolvedAt == null)
                            .ToListAsync();

                        if (dmcaList.Count != 0)
                        {
                            //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                            //   .Where(da => da.PodcastEpisodeId == episodeId
                            //   && da.ResolvedAt != null)
                            //   .ToListAsync();

                            var reason = DMCAAccusationDismissReasonEnum.ShowRemovedInDMCAAccusation;
                            string description = reason.GetDescription();

                            //if (dmcaAccusationExists.Count == 0)
                            //{
                                //Punish account
                            //    var accountPunishRequestData = new JObject
                            //{
                            //    { "AccountId", podcasterId },
                            //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                            //};
                            //    var accountPunishMessageName = "user-violation-punishment-flow";
                            //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //        topic: KafkaTopicEnum.ContentManagementDomain,
                            //        requestData: accountPunishRequestData,
                            //        sagaInstanceId: null,
                            //        messageName: accountPunishMessageName);
                            //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                            //}

                            var check = false;
                            foreach (var dmca in dmcaList)
                            {
                                var currentDmcaStatus = dmca.DmcaaccusationStatusTrackings
                                .OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault().DmcaAccusationStatusId;

                                if (currentDmcaStatus != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                {
                                    if (!check)
                                    {
                                        //var episode = await GetPodcastEpisode(parameter.PodcastEpisodeId);
                                        //var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                                        //var podcasterId = showFromEpisode.PodcasterId;

                                        //Punish account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);

                                        check = true;
                                    }
                                }

                                var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmca.Id,
                                    DmcaAccusationStatusId = currentDmcaStatus == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                dmca.DismissReason = description;
                                dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Show Episodes DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Show Episodes DMCA DMCA Remove Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Show Episodes DMCA DMCA Remove Show Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Show Episodes DMCA DMCA Remove Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissShowDMCAUnpublishShowForceAsync(DismissShowDMCAUnpublishShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;


                    var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da => da.PodcastShowId == parameter.PodcastShowId
                        && da.ResolvedAt == null)
                        .ToListAsync();

                    Guid? DmcaDismissedShowId = null;
                    if (dmcaList.Count != 0)
                    {
                        //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                        //.Where(da => da.PodcastShowId == parameter.PodcastShowId
                        //&& da.ResolvedAt != null)
                        //.ToListAsync();

                        var reason = DMCAAccusationDismissReasonEnum.ShowUnpublished;
                        string description = reason.GetDescription();

                        //if (dmcaAccusationExists.Count == 0)
                        //{
                        //    var show = await GetPodcastShow(parameter.PodcastShowId);
                        //    var podcasterId = show.PodcasterId;

                        //    //Punish account
                        //    var accountPunishRequestData = new JObject
                        //    {
                        //        { "AccountId", podcasterId },
                        //        { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                        //    };
                        //    var accountPunishMessageName = "user-violation-punishment-flow";
                        //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        //        topic: KafkaTopicEnum.ContentManagementDomain,
                        //        requestData: accountPunishRequestData,
                        //        sagaInstanceId: null,
                        //        messageName: accountPunishMessageName);
                        //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                        //}

                        var check = false;
                        foreach (var dmca in dmcaList)
                        {
                            var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                .OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault().DmcaAccusationStatusId;
                            if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                            {
                                if (!check)
                                {
                                    DmcaDismissedShowId = parameter.PodcastShowId;

                                    var show = await GetPodcastShow(parameter.PodcastShowId);
                                    if(show == null)
                                    {
                                        throw new HttpRequestException("Podcast Show not found for Podcast Show Id: " + parameter.PodcastShowId);
                                    }
                                    var podcasterId = show.PodcasterId;
                                    //Punish account
                                    var accountPunishRequestData = new JObject
                                    {
                                        { "AccountId", podcasterId },
                                        { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                    };
                                    var accountPunishMessageName = "user-violation-punishment-flow";
                                    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.UserManagementDomain,
                                        requestData: accountPunishRequestData,
                                        sagaInstanceId: null,
                                        messageName: accountPunishMessageName);
                                    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                    check = true;
                                }
                            }
                            var dismissedStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmca.Id,
                                DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                            dmca.DismissReason = description;
                            dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedShowId"] = DmcaDismissedShowId;
                    newRequestData["DmcaDismissedShowId"] = DmcaDismissedShowId;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Show DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Show DMCA Unpublish Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Show DMCA Unpublish Show Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Show DMCA Unpublish Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissShowEpisodesDMCAUnpublishShowForceAsync(DismissShowEpisodesDMCAUnpublishShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var show = await GetPodcastShowWithEpisode(parameter.PodcastShowId);
                    if(show == null)
                    {
                        throw new HttpRequestException("Podcast Show not found for Podcast Show Id: " + parameter.PodcastShowId);
                    }
                    var podcasterId = show.PodcasterId;

                    List<Guid> dismissedEpisodeIds = new List<Guid>();
                    foreach (var episodeId in show.PodcastEpisodes.Select(e => e.Id))
                    {
                        var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(da => da.DmcaaccusationStatusTrackings))
                            .Where(da => da.PodcastEpisodeId == episodeId
                            && da.ResolvedAt == null)
                            .ToListAsync();

                        if (dmcaList.Count != 0)
                        {
                            //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                            //   .Where(da => da.PodcastEpisodeId == episodeId
                            //   && da.ResolvedAt != null)
                            //   .ToListAsync();

                            var reason = DMCAAccusationDismissReasonEnum.ShowUnpublished;
                            string description = reason.GetDescription();

                            //if (dmcaAccusationExists.Count == 0)
                            //{
                            //    //Punish account
                            //    var accountPunishRequestData = new JObject
                            //{
                            //    { "AccountId", podcasterId },
                            //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                            //};
                            //    var accountPunishMessageName = "user-violation-punishment-flow";
                            //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //        topic: KafkaTopicEnum.ContentManagementDomain,
                            //        requestData: accountPunishRequestData,
                            //        sagaInstanceId: null,
                            //        messageName: accountPunishMessageName);
                            //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                            //}

                            var check = false;
                            foreach (var dmca in dmcaList)
                            {
                                var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                    .OrderByDescending(st => st.CreatedAt)
                                    .FirstOrDefault().DmcaAccusationStatusId;
                                if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                {
                                    if (!check)
                                    {
                                        dismissedEpisodeIds.Add(episodeId);

                                        // Punish account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                        check = true;
                                    }
                                }
                                var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmca.Id,
                                    DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                dmca.DismissReason = description;
                                dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    newRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Show Episodes DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Show Episodes DMCA Unpublish Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Show Episodes DMCA Unpublish Show Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Show Episodes DMCA Unpublish Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissChannelShowsDMCAUnpublishChannelForceAsync(DismissChannelShowsDMCAUnpublishChannelForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    List<Guid> dismissedShowId = new List<Guid>();
                    var showList = await GetPodcastShowByChannelId(parameter.PodcastChannelId);
                    //Console.WriteLine("Show List Count: " + showList.Count);
                    foreach (var show in showList)
                    {
                        var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(da => da.DmcaaccusationStatusTrackings))
                            .Where(da => da.PodcastShowId == show.Id
                            && da.ResolvedAt == null)
                            .ToListAsync();

                        if (dmcaList.Count != 0)
                        {

                            //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                            //.Where(da => da.PodcastShowId == show.Id
                            //&& da.ResolvedAt != null)
                            //.ToListAsync();

                            var reason = DMCAAccusationDismissReasonEnum.ChannelUnpublished;
                            string description = reason.GetDescription();

                            //if (dmcaAccusationExists.Count == 0)
                            //{
                            //    var podcasterId = show.PodcasterId;

                            //    //Punish account
                            //    var accountPunishRequestData = new JObject
                            //{
                            //    { "AccountId", podcasterId },
                            //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                            //};
                            //    var accountPunishMessageName = "user-violation-punishment-flow";
                            //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //        topic: KafkaTopicEnum.ContentManagementDomain,
                            //        requestData: accountPunishRequestData,
                            //        sagaInstanceId: null,
                            //        messageName: accountPunishMessageName);
                            //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                            //}

                            var check = false;
                            foreach (var dmca in dmcaList)
                            {
                                var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                    .OrderByDescending(st => st.CreatedAt)
                                    .FirstOrDefault().DmcaAccusationStatusId;
                                if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                {
                                    if (!check)
                                    {
                                        dismissedShowId.Add(show.Id);

                                        var podcasterId = show.PodcasterId;
                                        //Punish account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                        check = true;
                                    }
                                }
                                var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmca.Id,
                                    DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                dmca.DismissReason = description;
                                dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedShowIds"] = JArray.FromObject(dismissedShowId);
                    newRequestData["DmcaDismissedShowIds"] = JArray.FromObject(dismissedShowId);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Channel Shows DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Channel Shows DMCA Unpublish Channel Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Channel Shows DMCA Unpublish Channel Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Channel Shows DMCA Unpublish Channel Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissChannelEpisodesDMCAUnpublishChannelForceAsync(DismissChannelEpisodesDMCAUnpublishChannelForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    List<Guid> dismissedEpisodeIds = new List<Guid>();
                    var channel = await GetPodcastChannelWithShow(parameter.PodcastChannelId);
                    if(channel == null)
                    {
                        throw new HttpRequestException("Podcast Channel not found for Podcast Channel Id: " + parameter.PodcastChannelId);
                    }

                    if (channel.PodcastShows != null && channel.PodcastShows.Count > 0)
                    {
                        foreach (var showId in channel.PodcastShows.Select(s => s.Id))
                        {
                            var show = await GetPodcastShowWithEpisode(showId);
                            if(show == null)
                            {
                                //_logger.LogWarning("Podcast Show not found for Podcast Show Id: {ShowId}", showId);
                                //continue;
                                throw new HttpRequestException("Podcast Show not found for Podcast Show Id: " + showId);
                            }
                            var podcasterId = show.PodcasterId;

                            foreach (var episodeId in show.PodcastEpisodes.Select(e => e.Id))
                            {

                                var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                                    includeFunc: function => function
                                    .Include(da => da.DmcaaccusationStatusTrackings))
                                    .Where(da => da.PodcastEpisodeId == episodeId
                                    && da.ResolvedAt == null)
                                    .ToListAsync();

                                if (dmcaList.Count != 0)
                                {

                                    //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                                    //   .Where(da => da.PodcastEpisodeId == episodeId
                                    //   && da.ResolvedAt != null)
                                    //   .ToListAsync();

                                    var reason = DMCAAccusationDismissReasonEnum.ChannelUnpublished;
                                    string description = reason.GetDescription();

                                    //if (dmcaAccusationExists.Count == 0)
                                    //{
                                    //    //Punish account
                                    //    var accountPunishRequestData = new JObject
                                    //{
                                    //    { "AccountId", podcasterId },
                                    //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                    //};
                                    //    var accountPunishMessageName = "user-violation-punishment-flow";
                                    //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    //        topic: KafkaTopicEnum.ContentManagementDomain,
                                    //        requestData: accountPunishRequestData,
                                    //        sagaInstanceId: null,
                                    //        messageName: accountPunishMessageName);
                                    //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                    //}

                                    var check = false;
                                    foreach (var dmca in dmcaList)
                                    {
                                        var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                            .OrderByDescending(st => st.CreatedAt)
                                            .FirstOrDefault().DmcaAccusationStatusId;
                                        if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                        {
                                            if (!check)
                                            {
                                                dismissedEpisodeIds.Add(episodeId);
                                                //Punish account
                                                var accountPunishRequestData = new JObject
                                                {
                                                    { "AccountId", podcasterId },
                                                    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                                };
                                                var accountPunishMessageName = "user-violation-punishment-flow";
                                                var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                                    topic: KafkaTopicEnum.UserManagementDomain,
                                                    requestData: accountPunishRequestData,
                                                    sagaInstanceId: null,
                                                    messageName: accountPunishMessageName);
                                                await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                                check = true;
                                            }
                                        }
                                        var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                        {
                                            DmcaAccusationId = dmca.Id,
                                            DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                        };
                                        await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                        dmca.DismissReason = description;
                                        dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                        dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                        await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                                    }
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    newRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Channel Episodes DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Channel Episodes DMCA Unpublish Channel Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Channel Episodes DMCA Unpublish Channel Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Channel Episodes DMCA Unpublish Channel Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissEpisodeDMCAEpisodeDeletionForceAsync(DismissEpisodeDMCAEpisodeDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da => da.PodcastEpisodeId == parameter.PodcastEpisodeId
                        && da.ResolvedAt == null)
                        .ToListAsync();

                    Guid? DmcaDismissedEpisodeId = null;
                    if (dmcaList.Count != 0)
                    {
                        //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                        //    .Where(da => da.PodcastEpisodeId == parameter.PodcastEpisodeId
                        //    && da.ResolvedAt != null)
                        //    .ToListAsync();

                        var reason = DMCAAccusationDismissReasonEnum.EpisodeDeleted;
                        string description = reason.GetDescription();

                        //if (dmcaAccusationExists.Count == 0)
                        //{
                        //    var episode = await GetPodcastEpisode(parameter.PodcastEpisodeId);
                        //    var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                        //    var podcasterId = showFromEpisode.PodcasterId;

                        //    //Punish account
                        //    var accountPunishRequestData = new JObject
                        //{
                        //    { "AccountId", podcasterId },
                        //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                        //};
                        //    var accountPunishMessageName = "user-violation-punishment-flow";
                        //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        //        topic: KafkaTopicEnum.ContentManagementDomain,
                        //        requestData: accountPunishRequestData,
                        //        sagaInstanceId: null,
                        //        messageName: accountPunishMessageName);
                        //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                        //}

                        var check = false;
                        foreach (var dmca in dmcaList)
                        {
                            var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                .OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault().DmcaAccusationStatusId;
                            if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                            {
                                if (!check)
                                {
                                    DmcaDismissedEpisodeId = parameter.PodcastEpisodeId;

                                    var episode = await GetPodcastEpisodeWithShow(parameter.PodcastEpisodeId);
                                    if(episode == null)
                                    {
                                        throw new HttpRequestException("Podcast Episode not found for Podcast Episode Id: " + parameter.PodcastEpisodeId);
                                    }
                                    var podcasterId = 0;
                                    if (episode.PodcastShow != null)
                                    {
                                        podcasterId = episode.PodcastShow.PodcasterId;
                                    }
                                    else
                                    {
                                        var showFromEpisode = await GetPodcastShow(episode.PodcastShowId);
                                        if(showFromEpisode == null)
                                        {
                                            throw new HttpRequestException("Podcast Show not found for Podcast Show Id: " + episode.PodcastShowId);
                                        }
                                        podcasterId = showFromEpisode.PodcasterId;
                                    }

                                    if(podcasterId != 0)
                                    {
                                        //Punish account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                    }
                                    else
                                    {
                                        //_logger.LogWarning("PodcasterId is 0 for EpisodeId: {EpisodeId}", parameter.PodcastEpisodeId);
                                        throw new HttpRequestException("Podcaster Id is invalid for Podcast Episode Id: " + parameter.PodcastEpisodeId);
                                    }

                                    check = true;
                                }
                            }
                            var dismissedStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmca.Id,
                                DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                            dmca.DismissReason = description;
                            dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedEpisodeId"] = DmcaDismissedEpisodeId;
                    newRequestData["DmcaDismissedEpisodeId"] = DmcaDismissedEpisodeId;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Episode DMCA Episode Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Episode DMCA Episode Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Episode DMCA Episode Deletion Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Episode DMCA Episode Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissShowDMCAShowDeletionForceAsync(DismissShowDMCAShowDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings))
                        .Where(da => da.PodcastShowId == parameter.PodcastShowId
                        && da.ResolvedAt == null)
                        .ToListAsync();

                    Guid? DmcaDismissedShowId = null;
                    if (dmcaList.Count != 0)
                    {
                        //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                        //    .Where(da => da.PodcastShowId == parameter.PodcastShowId
                        //    && da.ResolvedAt != null)
                        //    .ToListAsync();

                        var reason = DMCAAccusationDismissReasonEnum.ShowDeleted;
                        string description = reason.GetDescription();

                        //if (dmcaAccusationExists.Count == 0)
                        //{
                        //    var show = await GetPodcastShow(parameter.PodcastShowId);
                        //    var podcasterId = show.PodcasterId;

                        //    //Punish account
                        //    var accountPunishRequestData = new JObject
                        //{
                        //    { "AccountId", podcasterId },
                        //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                        //};
                        //    var accountPunishMessageName = "user-violation-punishment-flow";
                        //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        //        topic: KafkaTopicEnum.ContentManagementDomain,
                        //        requestData: accountPunishRequestData,
                        //        sagaInstanceId: null,
                        //        messageName: accountPunishMessageName);
                        //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                        //}

                        var check = false;
                        foreach (var dmca in dmcaList)
                        {
                            var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                .OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault().DmcaAccusationStatusId;
                            if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                            {
                                if (!check)
                                {
                                    DmcaDismissedShowId = parameter.PodcastShowId;

                                    var show = await GetPodcastShow(parameter.PodcastShowId);
                                    if(show == null)
                                    {
                                        throw new HttpRequestException($"Podcast Show with Id {parameter.PodcastShowId} not found.");
                                    }
                                    var podcasterId = show.PodcasterId;

                                    //Punish account
                                    var accountPunishRequestData = new JObject
                                    {
                                        { "AccountId", podcasterId },
                                        { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                    };
                                    var accountPunishMessageName = "user-violation-punishment-flow";
                                    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.UserManagementDomain,
                                        requestData: accountPunishRequestData,
                                        sagaInstanceId: null,
                                        messageName: accountPunishMessageName);
                                    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                    check = true;
                                }
                            }
                            var dismissedStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmca.Id,
                                DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                            dmca.DismissReason = description;
                            dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedShowId"] = DmcaDismissedShowId;
                    newRequestData["DmcaDismissedShowId"] = DmcaDismissedShowId;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Show DMCA Show Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Show DMCA Show Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Show DMCA Show Deletion Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Show DMCA Show Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissShowEpisodesDMCAShowDeletionForceAsync(DismissShowEpisodesDMCAShowDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var show = await GetPodcastShowWithEpisode(parameter.PodcastShowId);
                    if(show == null)
                    {
                        throw new HttpRequestException($"Podcast Show with Id {parameter.PodcastShowId} not found.");
                    }
                    var podcasterId = show.PodcasterId;

                    List<Guid> dismissedEpisodeIds = new List<Guid>();
                    foreach (var episodeId in show.PodcastEpisodes.Select(e => e.Id))
                    {
                        var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(da => da.DmcaaccusationStatusTrackings))
                            .Where(da => da.PodcastEpisodeId == episodeId
                            && da.ResolvedAt == null)
                            .ToListAsync();

                        if (dmcaList.Count != 0)
                        {

                            //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                            //   .Where(da => da.PodcastEpisodeId == episodeId
                            //   && da.ResolvedAt != null)
                            //   .ToListAsync();

                            var reason = DMCAAccusationDismissReasonEnum.ShowDeleted;
                            string description = reason.GetDescription();

                            //if (dmcaAccusationExists.Count == 0)
                            //{
                            //    //Punish account
                            //    var accountPunishRequestData = new JObject
                            //{
                            //    { "AccountId", podcasterId },
                            //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                            //};
                            //    var accountPunishMessageName = "user-violation-punishment-flow";
                            //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //        topic: KafkaTopicEnum.ContentManagementDomain,
                            //        requestData: accountPunishRequestData,
                            //        sagaInstanceId: null,
                            //        messageName: accountPunishMessageName);
                            //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                            //}

                            var check = false;
                            foreach (var dmca in dmcaList)
                            {
                                var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                    .OrderByDescending(st => st.CreatedAt)
                                    .FirstOrDefault().DmcaAccusationStatusId;
                                if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                {
                                    if (!check)
                                    {
                                        dismissedEpisodeIds.Add(episodeId);

                                        // Punish Account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);

                                        check = true;
                                    }
                                }

                                var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmca.Id,
                                    DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                dmca.DismissReason = description;
                                dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    newRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Show DMCA Show Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Show Episodes DMCA Show Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Show Episodes DMCA Show Deletion Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Show Episodes DMCA Show Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissChannelShowsDMCAChannelDeletionForceAsync(DismissChannelShowsDMCAChannelDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    List<Guid> dismissedShowId = new List<Guid>();
                    var showList = await GetPodcastShowByChannelId(parameter.PodcastChannelId);
                    foreach (var show in showList)
                    {
                        var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(da => da.DmcaaccusationStatusTrackings))
                            .Where(da => da.PodcastShowId == show.Id
                            && da.ResolvedAt == null)
                            .ToListAsync();

                        if (dmcaList.Count != 0)
                        {
                            //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                            //.Where(da => da.PodcastShowId == show.Id
                            //&& da.ResolvedAt != null)
                            //.ToListAsync();

                            var reason = DMCAAccusationDismissReasonEnum.ChannelDeleted;
                            string description = reason.GetDescription();

                            //if (dmcaAccusationExists.Count == 0)
                            //{
                            //    var podcasterId = show.PodcasterId;

                            //    //Punish account
                            //    var accountPunishRequestData = new JObject
                            //    {
                            //        { "AccountId", podcasterId },
                            //        { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                            //    };
                            //    var accountPunishMessageName = "user-violation-punishment-flow";
                            //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //        topic: KafkaTopicEnum.ContentManagementDomain,
                            //        requestData: accountPunishRequestData,
                            //        sagaInstanceId: null,
                            //        messageName: accountPunishMessageName);
                            //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                            //}

                            var check = false;
                            foreach (var dmca in dmcaList)
                            {
                                var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                    .OrderByDescending(st => st.CreatedAt)
                                    .FirstOrDefault().DmcaAccusationStatusId;
                                if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                {
                                    if (!check)
                                    {
                                        dismissedShowId.Add(show.Id);
                                        var podcasterId = show.PodcasterId;

                                        //Punish account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                        check = true;
                                    }
                                }
                                var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmca.Id,
                                    DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                dmca.DismissReason = description;
                                dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedShowIds"] = JArray.FromObject(dismissedShowId);
                    newRequestData["DmcaDismissedShowIds"] = JArray.FromObject(dismissedShowId);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Channel Shows DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Channel Shows DMCA Channel Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Channel Shows DMCA Channel Deletion Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Channel Shows DMCA Channel Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissChannelEpisodesDMCAChannelDeletionForceAsync(DismissChannelEpisodesDMCAChannelDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    List<Guid> dismissedEpisodeIds = new List<Guid>();
                    var channel = await GetPodcastChannelWithShow(parameter.PodcastChannelId);
                    if(channel == null)
                    {
                        throw new HttpRequestException($"Podcast Channel with Id {parameter.PodcastChannelId} not found.");
                    }
                    if (channel.PodcastShows != null && channel.PodcastShows.Count > 0)
                    {
                        foreach (var showId in channel.PodcastShows.Select(s => s.Id))
                        {
                            var show = await GetPodcastShowWithEpisode(showId);
                            if(show == null)
                            {
                                throw new HttpRequestException($"Podcast Show with Id {showId} not found.");
                            }
                            var podcasterId = show.PodcasterId;

                            foreach (var episodeId in show.PodcastEpisodes.Select(e => e.Id))
                            {
                                var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                                    includeFunc: function => function
                                    .Include(da => da.DmcaaccusationStatusTrackings))
                                    .Where(da => da.PodcastEpisodeId == episodeId
                                    && da.ResolvedAt == null)
                                    .ToListAsync();

                                if (dmcaList.Count != 0)
                                {
                                    //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                                    //   .Where(da => da.PodcastEpisodeId == episodeId
                                    //   && da.ResolvedAt != null)
                                    //   .ToListAsync();

                                    var reason = DMCAAccusationDismissReasonEnum.ChannelDeleted;
                                    string description = reason.GetDescription();

                                    //if (dmcaAccusationExists.Count == 0)
                                    //{
                                    //    //Punish account
                                    //    var accountPunishRequestData = new JObject
                                    //{
                                    //    { "AccountId", podcasterId },
                                    //    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                    //};
                                    //    var accountPunishMessageName = "user-violation-punishment-flow";
                                    //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    //        topic: KafkaTopicEnum.ContentManagementDomain,
                                    //        requestData: accountPunishRequestData,
                                    //        sagaInstanceId: null,
                                    //        messageName: accountPunishMessageName);
                                    //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                    //}

                                    var check = false;
                                    foreach (var dmca in dmcaList)
                                    {
                                        var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                            .OrderByDescending(st => st.CreatedAt)
                                            .FirstOrDefault().DmcaAccusationStatusId;
                                        if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                        {
                                            if (!check)
                                            {
                                                dismissedEpisodeIds.Add(episodeId);

                                                //Punish account
                                                var accountPunishRequestData = new JObject
                                                {
                                                    { "AccountId", podcasterId },
                                                    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                                };
                                                var accountPunishMessageName = "user-violation-punishment-flow";
                                                var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                                    topic: KafkaTopicEnum.UserManagementDomain,
                                                    requestData: accountPunishRequestData,
                                                    sagaInstanceId: null,
                                                    messageName: accountPunishMessageName);
                                                await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);

                                                check = true;
                                            }
                                        }
                                        var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                        {
                                            DmcaAccusationId = dmca.Id,
                                            DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                        };
                                        await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                        dmca.DismissReason = description;
                                        dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                        dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                        await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                                    }
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    newRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Channel Episodes DMCA Accusation for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Channel Episodes DMCA Channel Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Channel Episodes DMCA Channel Deletion Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Channel Episodes DMCA Channel Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissPodcasterShowsDMCATerminatePodcasterForceAsync(DismissPodcasterShowsDMCATerminatePodcasterForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    List<Guid> dismissedShowIds = new List<Guid>();
                    var showList = await GetPodcastShowByPodcasterId(parameter.PodcasterId);
                    foreach (var show in showList)
                    {
                        var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                            includeFunc: function => function
                            .Include(da => da.DmcaaccusationStatusTrackings))
                            .Where(da => da.PodcastShowId == show.Id
                            && da.ResolvedAt == null)
                            .ToListAsync();

                        if (dmcaList.Count != 0)
                        {
                            //var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                            //    .Where(da => da.PodcastShowId == show.Id
                            //    && da.ResolvedAt != null)
                            //    .ToListAsync();

                            var reason = DMCAAccusationDismissReasonEnum.PodcasterDeactivated;
                            string description = reason.GetDescription();

                            //if (dmcaAccusationExists.Count == 0)
                            //{
                            //    var podcasterId = show.PodcasterId;

                            //    //Punish account
                            //    var accountPunishRequestData = new JObject
                            //    {
                            //        { "AccountId", podcasterId },
                            //        { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                            //    };
                            //    var accountPunishMessageName = "user-violation-punishment-flow";
                            //    var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            //        topic: KafkaTopicEnum.ContentManagementDomain,
                            //        requestData: accountPunishRequestData,
                            //        sagaInstanceId: null,
                            //        messageName: accountPunishMessageName);
                            //    await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                            //}

                            var check = false;
                            foreach (var dmca in dmcaList)
                            {
                                var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                    .OrderByDescending(st => st.CreatedAt)
                                    .FirstOrDefault().DmcaAccusationStatusId;
                                if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                {
                                    if (!check)
                                    {
                                        dismissedShowIds.Add(show.Id);

                                        var podcasterId = show.PodcasterId;

                                        //Punish account
                                        var accountPunishRequestData = new JObject
                                        {
                                            { "AccountId", podcasterId },
                                            { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                        };
                                        var accountPunishMessageName = "user-violation-punishment-flow";
                                        var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.UserManagementDomain,
                                            requestData: accountPunishRequestData,
                                            sagaInstanceId: null,
                                            messageName: accountPunishMessageName);
                                        await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);

                                        check = true;
                                    }
                                }
                                var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmca.Id,
                                    DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                dmca.DismissReason = description;
                                dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedShowIds"] = JArray.FromObject(dismissedShowIds);
                    newRequestData["DmcaDismissedShowIds"] = JArray.FromObject(dismissedShowIds);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Podcaster Shows DMCA Terminate Podcaster Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Podcaster Shows DMCA Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Podcaster Shows DMCA Terminate Podcaster Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Podcaster Shows DMCA Terminate Podcaster Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DismissPodcasterEpisodesDMCATerminatePodcasterForceAsync(DismissPodcasterEpisodesDMCATerminatePodcasterForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    List<Guid> dismissedEpisodeIds = new List<Guid>();
                    var showList = await GetPodcastShowByPodcasterId(parameter.PodcasterId);
                    if (showList != null && showList.Count > 0)
                    {
                        foreach (var show in showList)
                        {
                            var podcasterId = show.PodcasterId;

                            foreach (var episodeId in show.PodcastEpisodes.Select(e => e.Id))
                            {
                                var dmcaList = await _dmcaAccusationGenericRepository.FindAll(
                                    includeFunc: function => function
                                    .Include(da => da.DmcaaccusationStatusTrackings)
                                )
                                    .Where(da => da.PodcastEpisodeId == episodeId
                                    && da.ResolvedAt == null)
                                    .ToListAsync();

                                if (dmcaList.Count != 0)
                                {

                                    // var dmcaAccusationExists = await _dmcaAccusationGenericRepository.FindAll()
                                    //    .Where(da => da.PodcastEpisodeId == episodeId
                                    //    && da.ResolvedAt != null)
                                    //    .ToListAsync();

                                    var reason = DMCAAccusationDismissReasonEnum.PodcasterDeactivated;
                                    string description = reason.GetDescription();

                                //     if (dmcaAccusationExists.Count == 0)
                                //     {
                                //         //Punish account
                                //         var accountPunishRequestData = new JObject
                                // {
                                //     { "AccountId", podcasterId },
                                //     { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                // };
                                //         var accountPunishMessageName = "user-violation-punishment-flow";
                                //         var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                //             topic: KafkaTopicEnum.ContentManagementDomain,
                                //             requestData: accountPunishRequestData,
                                //             sagaInstanceId: null,
                                //             messageName: accountPunishMessageName);
                                //         await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);
                                //     }

                                    var check = false;
                                    foreach (var dmca in dmcaList)
                                    {
                                        var currentStatusId = dmca.DmcaaccusationStatusTrackings
                                            .OrderByDescending(st => st.CreatedAt)
                                            .FirstOrDefault().DmcaAccusationStatusId;
                                        if(currentStatusId != (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview)
                                        {
                                            if (!check)
                                            {
                                                dismissedEpisodeIds.Add(episodeId);

                                                // Punish Account
                                                var accountPunishRequestData = new JObject
                                                {
                                                    { "AccountId", podcasterId },
                                                    { "ViolationPoint", _dmcaAccusationConfig.DismissStaticViolationPoint }
                                                };
                                                var accountPunishMessageName = "user-violation-punishment-flow";
                                                var sagaAccountPunishStartSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                                    topic: KafkaTopicEnum.UserManagementDomain,
                                                    requestData: accountPunishRequestData,
                                                    sagaInstanceId: null,
                                                    messageName: accountPunishMessageName);
                                                await _messagingService.SendSagaMessageAsync(sagaAccountPunishStartSagaTriggerMessage);

                                                check = true;
                                            }
                                        }
                                        var dismissedStatusTracking = new DmcaaccusationStatusTracking
                                        {
                                            DmcaAccusationId = dmca.Id,
                                            DmcaAccusationStatusId = currentStatusId == (int)DMCAAccusationStatusEnum.PendingDMCANoticeReview ? (int)DMCAAccusationStatusEnum.UnresolvedDismissed : (int)DMCAAccusationStatusEnum.DirectResolveDismissed,
                                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                        };
                                        await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(dismissedStatusTracking);

                                        dmca.DismissReason = description;
                                        dmca.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                        dmca.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                        await _dmcaAccusationGenericRepository.UpdateAsync(dmca.Id, dmca);
                                    }
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newRequestData = command.RequestData;
                    newResponseData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    newRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(dismissedEpisodeIds);
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully dismiss Podcaster Episodes DMCA Terminate Podcaster Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Dismiss Podcaster Episodes DMCA Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Dismiss Podcaster Episodes DMCA Terminate Podcaster Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.DmcaManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Dismiss Podcaster Episodes DMCA Terminate Podcaster Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<List<DMCAAccusationConclusionReportListItemResponseDTO>> GetAllDMCAAccusationConclusionReportsForStaffOrAdminAsync(int accountId, int roleId)
        {
            try
            {
                var query = _dmcaAccusationConclusionReportGenericRepository.FindAll(
                    includeFunc: function => function
                        .Include(dacr => dacr.DmcaAccusation)
                        .Include(dacr => dacr.DmcaAccusationConclusionReportType)
                );

                if (roleId == (int)RoleEnum.Staff)
                {
                    query = query.Where(dacr => dacr.DmcaAccusation.AssignedStaff == accountId);
                }
                return await query
                    .OrderByDescending(dacr => dacr.CreatedAt)
                    .Select(dacr => new DMCAAccusationConclusionReportListItemResponseDTO
                    {
                        Id = dacr.Id,
                        DmcaAccusationId = dacr.DmcaAccusationId,
                        Description = dacr.Description,
                        InvalidReason = dacr.InvalidReason,
                        IsRejected = dacr.IsRejected,
                        CompletedAt = dacr.CompletedAt,
                        CancelledAt = dacr.CancelledAt,
                        CreatedAt = dacr.CreatedAt,
                        UpdatedAt = dacr.UpdatedAt,
                        DmcaAccusationConclusionReportType = new DMCAAccusationConclusionReportTypeResponseDTO
                        {
                            Id = dacr.DmcaAccusationConclusionReportType.Id,
                            Name = dacr.DmcaAccusationConclusionReportType.Name
                        }
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all DMCA Accusation Conclusion Reports for Staff or Admin with AccountId: {AccountId}", accountId);
                throw new HttpRequestException("Error occurred while getting all DMCA Accusation Conclusion Reports for Staff or Admin. Error: " + ex.Message);
            }
        }
        public async Task<List<DMCAAccusationConclusionReportListItemResponseDTO>> GetAllDMCAAccusationConclusionReportsByAccusationIdForStaffOrAdminAsync(int dmcaAccusationId, int accountId, int roleId)
        {
            try
            {
                var query = _dmcaAccusationConclusionReportGenericRepository.FindAll(
                    includeFunc: function => function
                        .Include(dacr => dacr.DmcaAccusation)
                        .Include(dacr => dacr.DmcaAccusationConclusionReportType)
                );

                if (roleId == (int)RoleEnum.Staff)
                {
                    query = query.Where(dacr => dacr.DmcaAccusation.AssignedStaff == accountId);
                }
                return await query
                    .Where(dacr => dacr.DmcaAccusationId == dmcaAccusationId)
                    .OrderByDescending(dacr => dacr.CreatedAt)
                    .Select(dacr => new DMCAAccusationConclusionReportListItemResponseDTO
                    {
                        Id = dacr.Id,
                        DmcaAccusationId = dacr.DmcaAccusationId,
                        Description = dacr.Description,
                        InvalidReason = dacr.InvalidReason,
                        IsRejected = dacr.IsRejected,
                        CompletedAt = dacr.CompletedAt,
                        CancelledAt = dacr.CancelledAt,
                        CreatedAt = dacr.CreatedAt,
                        UpdatedAt = dacr.UpdatedAt,
                        DmcaAccusationConclusionReportType = new DMCAAccusationConclusionReportTypeResponseDTO
                        {
                            Id = dacr.DmcaAccusationConclusionReportType.Id,
                            Name = dacr.DmcaAccusationConclusionReportType.Name
                        }
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all DMCA Accusation Conclusion Reports for DMCA Accusation Id: {DmcaAccusationId}", dmcaAccusationId);
                throw new HttpRequestException("Error occurred while getting all DMCA Accusation Conclusion Reports. Error: " + ex.Message);
            }
        }
        private async Task<(bool isValid, string errorMessage)> ValidateEpisode(Guid podcastEpisodeId)
        {
            var episode = await GetPodcastEpisode(podcastEpisodeId);
            if (episode == null)
            {
                return (false, $"Podcast episode with Id: {podcastEpisodeId} is not found");
            }
            var (isValid, errorMessage) = await ValidateShow(episode.PodcastShowId, podcastEpisodeId);
            if (!isValid)
            {
                return (isValid, errorMessage);
            }
            if (episode.DeletedAt != null)
            {
                return (false, $"Podcast episode with Id: {podcastEpisodeId} has already been deleted");
            }
            var episodeStatusId = episode.PodcastEpisodeStatusTrackings.OrderByDescending(es => es.CreatedAt).Select(es => es.PodcastEpisodeStatusId).First();
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.Draft)
            {
                return (false, $"Podcast episode with Id: {podcastEpisodeId} is in Draft status");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.PendingReview)
            {
                return (false, $"Podcast episode with Id: {podcastEpisodeId} is pending review");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.PendingEditRequired)
            {
                return (false, $"Podcast episode with Id: {podcastEpisodeId} is pending edit required");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown)
            {
                return (false, $"Podcast episode with Id: {podcastEpisodeId} has been taken down");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.Removed)
            {
                return (false, $"Podcast episode with Id: {podcastEpisodeId} has been removed");
            }
            return (true, string.Empty);
        }
        private async Task<(bool isValid, string errorMessage)> ValidateShow(Guid podcastShowId, Guid? podcastEpisodeId = null)
        {
            var insideMessage = podcastEpisodeId != null ? $" for Episode Id: {podcastEpisodeId}" : string.Empty;
            var show = await GetPodcastShow(podcastShowId);
            if (show == null)
            {
                return (false, $"Podcast show with Id: {podcastShowId} is not found {insideMessage}");
            }
            if (show.PodcastChannelId != null)
            {
                var (isValid, errorMessage) = await ValidateChannel(show.PodcastChannelId.Value, podcastShowId, podcastEpisodeId);
                if (!isValid)
                {
                    return (isValid, errorMessage);
                }
            }
            if (show.DeletedAt != null)
            {
                return (false, $"Podcast show with Id: {podcastShowId} has already been deleted {insideMessage}");
            }
            var showStatusId = show.PodcastShowStatusTrackings.OrderByDescending(ss => ss.CreatedAt).Select(ss => ss.PodcastShowStatusId).First();
            if (showStatusId == (int)PodcastShowStatusEnum.Draft)
            {
                return (false, $"Podcast show with Id: {podcastShowId} is in Draft status {insideMessage}");
            }
            if (showStatusId == (int)PodcastShowStatusEnum.TakenDown)
            {
                return (false, $"Podcast show with Id: {podcastShowId} has been taken down {insideMessage}");
            }
            if (showStatusId == (int)PodcastShowStatusEnum.Removed)
            {
                return (false, $"Podcast show with Id: {podcastShowId} has been removed {insideMessage}");
            }
            return (true, string.Empty);
        }
        private async Task<(bool isValid, string errorMessage)> ValidateChannel(Guid podcastChannelId, Guid? podcastShowId = null, Guid? podcastEpisodeId = null)
        {
            var insideMessage = podcastEpisodeId != null
                ? $" for Episode Id: {podcastEpisodeId}"
                : (podcastShowId != null
                    ? $" for Show Id: {podcastShowId}"
                    : string.Empty);
            var channel = await GetPodcastChannel(podcastChannelId);
            if (channel == null)
            {
                return (false, $"Podcast channel with Id: {podcastChannelId} is not found {insideMessage}");
            }
            if (channel.DeletedAt != null)
            {
                return (false, $"Podcast channel with Id: {podcastChannelId} has already been deleted {insideMessage}");
            }
            var channelStatusId = channel.PodcastChannelStatusTrackings.OrderByDescending(cs => cs.CreatedAt).Select(cs => cs.PodcastChannelStatusId).First();
            if (channelStatusId == (int)PodcastChannelStatusEnum.Unpublished)
            {
                return (false, $"Podcast channel with Id: {podcastChannelId} is in Draft status {insideMessage}");
            }
            return (true, string.Empty);
        }
        public async Task<PodcastEpisodeDTO?> GetPodcastEpisode(Guid podcastEpisodeId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastEpisode",
                            QueryType = "findall",
                            EntityType = "PodcastEpisode",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = podcastEpisodeId
                                },
                                include = "PodcastEpisodeStatusTrackings"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastEpisode"] is JArray podcastEpisodeArray && podcastEpisodeArray.Count > 0
                    ? podcastEpisodeArray.First.ToObject<PodcastEpisodeDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Episode with Id: {PodcastEpisodeId}", podcastEpisodeId);
                throw new HttpRequestException("Get Podcast Episode by id failed, error: " + ex.Message); ;
            }
        }
        public async Task<PodcastEpisodeWithShowDTO?> GetPodcastEpisodeWithShow(Guid podcastEpisodeId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastEpisode",
                            QueryType = "findall",
                            EntityType = "PodcastEpisode",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = podcastEpisodeId
                                },
                                include = "PodcastEpisodeStatusTrackings, PodcastShow"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastEpisode"] is JArray podcastEpisodeArray && podcastEpisodeArray.Count > 0
                    ? podcastEpisodeArray.First.ToObject<PodcastEpisodeWithShowDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Episode with Id: {PodcastEpisodeId}", podcastEpisodeId);
                throw new HttpRequestException("Get Podcast Episode by id failed, error: " + ex.Message); ;
            }
        }
        public async Task<List<PodcastEpisodeDTO>?> GetPodcastEpisodeByShowId(Guid podcastShowId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastEpisode",
                            QueryType = "findall",
                            EntityType = "PodcastEpisode",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcastShowId = podcastShowId
                                },
                                include = "PodcastEpisodeStatusTrackings"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastEpisode"] is JArray podcastEpisodeArray && podcastEpisodeArray.Count >= 0
                    ? podcastEpisodeArray.ToObject<List<PodcastEpisodeDTO>>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Episodes for Show Id: {PodcastShowId}", podcastShowId);
                throw new HttpRequestException("Get Podcast Episodes for Show Id failed, error: " + ex.Message);
            }
        }
        public async Task<PodcastChannelDTO?> GetPodcastChannel(Guid podcastChannelId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastChannel",
                            QueryType = "findall",
                            EntityType = "PodcastChannel",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = podcastChannelId
                                },
                                include = "PodcastChannelStatusTrackings"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastChannel"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                    ? podcastChannelArray.First.ToObject<PodcastChannelDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Channel with Id: {PodcastChannelId}", podcastChannelId);
                throw new HttpRequestException("Get Podcast Channel with Id failed, error " + ex.Message);
            }
        }
        public async Task<List<PodcastChannelDTO>?> GetPodcastChannelByPodcasterId(int podcasterId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastChannels",
                            QueryType = "findall",
                            EntityType = "PodcastChannel",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcasterId = podcasterId
                                },
                                include = "PodcastChannelStatusTrackings"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastChannels"] is JArray podcastChannelArray && podcastChannelArray.Count >= 0
                    ? podcastChannelArray.ToObject<List<PodcastChannelDTO>>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Channels for Podcaster Id: {PodcasterId}", podcasterId);
                throw new HttpRequestException("Get Podcast Channels for Podcaster Id failed, error: " + ex.Message);
            }
        }
        public async Task<PodcastShowDTO?> GetPodcastShow(Guid podcastShowId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastShow",
                            QueryType = "findall",
                            EntityType = "PodcastShow",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = podcastShowId
                                },
                                include = "PodcastShowStatusTrackings"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count > 0
                    ? podcastShowArray.First.ToObject<PodcastShowDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Show with Id: {PodcastShowId}", podcastShowId);
                throw new HttpRequestException("Get Podcast Show with Id failed, error: " + ex.Message);
            }
        }
        public async Task<List<PodcastShowDTO>?> GetPodcastShowByChannelId(Guid podcastChannelid)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastShow",
                            QueryType = "findall",
                            EntityType = "PodcastShow",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcastChannelId = podcastChannelid
                                },
                                include = "PodcastShowStatusTrackings"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count >= 0
                    ? podcastShowArray.ToObject<List<PodcastShowDTO>>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Shows for Channel Id: {PodcastChannelId}", podcastChannelid);
                throw new HttpRequestException("Get Podcast Shows for Channel Id failed, error: " + ex.Message);
            }
        }
        public async Task<List<PodcastShowWithEpisodeDTO>?> GetPodcastShowByPodcasterId(int podcasterId)
        {
            try
            {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastShow",
                            QueryType = "findall",
                            EntityType = "PodcastShow",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcasterId = podcasterId
                                },
                                include = "PodcastShowStatusTrackings, PodcastEpisodes"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count >= 0
                ? podcastShowArray.ToObject<List<PodcastShowWithEpisodeDTO>>()
                : null;
            } catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Shows for Podcaster Id: {PodcasterId}", podcasterId);
                throw new HttpRequestException("Get Podcast Shows for Podcaster Id failed, error: " + ex.Message);
            }
        }
        public async Task<DmcaaccusationStatusTracking> CreateDMCAAccusationStatusTracking(DmcaaccusationStatusTracking status)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var dmcaAccusation = await _dmcaAccusationGenericRepository.FindByIdAsync(status.DmcaAccusationId);
                    var result = await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(status);
                    dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                    await transaction.CommitAsync();
                    _logger.LogInformation("Created DMCA Accusation Status Tracking with ID: {DmcaAccusationStatusTrackingId}", result.Id);
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Something went wrong. Error: {ex.StackTrace}");
                    return null;
                }
            }
        }
        private async Task<SystemConfigProfileDTO?> GetActiveSystemConfigProfile()
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeSystemConfigProfile",
                            QueryType = "findall",
                            EntityType = "SystemConfigProfile",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsActive = true
                                    },
                                    include = "AccountConfig,AccountViolationLevelConfigs, BookingConfig, PodcastSubscriptionConfigs, PodcastSuggestionConfig, ReviewSessionConfig",

                                }),
                            Fields = new[] { "Id", "Name", "IsActive", "AccountConfig", "AccountViolationLevelConfigs", "BookingConfig", "PodcastSubscriptionConfigs", "PodcastSuggestionConfig", "ReviewSessionConfig" }
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

                return result.Results?["activeSystemConfigProfile"] is JArray configArray && configArray.Count > 0
                    ? configArray.First.ToObject<SystemConfigProfileDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Active System Config Profile");
                throw new HttpRequestException("Get Active System Config Profile failed, error: " + ex.Message);
            }
        }
        public async Task<PodcastChannelWithShowDTO?> GetPodcastChannelWithShow(Guid podcastChannelId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastChannel",
                            QueryType = "findall",
                            EntityType = "PodcastChannel",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = podcastChannelId
                                },
                                include = "PodcastChannelStatusTrackings, PodcastShows"
                            })
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastChannel"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                    ? podcastChannelArray.First.ToObject<PodcastChannelWithShowDTO>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Channel with Shows for Channel Id: {PodcastChannelId}", podcastChannelId);
                throw new HttpRequestException("Get Podcast Channel with Shows for Channel Id failed, error: " + ex.Message);
            }
        }
        public async Task<PodcastShowWithEpisodeDTO?> GetPodcastShowWithEpisode(Guid podcastShowId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "podcastShow",
                                QueryType = "findall",
                                EntityType = "PodcastShow",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        Id = podcastShowId
                                    },
                                    include = "PodcastShowStatusTrackings, PodcastEpisodes"
                                })
                            }
                        }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

                return result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count > 0
                    ? podcastShowArray.First.ToObject<PodcastShowWithEpisodeDTO>()
                    : null;
            } catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while getting Podcast Show with Episodes for Show Id: {PodcastShowId}", podcastShowId);
                throw new HttpRequestException("Get Podcast Show with Episodes for Show Id failed, error: " + ex.Message);
            }
        }
        public bool IsValidFile(string fileName, long fileSizeBytes, string mimeType)
        {
            var rule = new FileValidationRule
            {
                MaxSizeBytes = 52428800,
                AllowedExtensions = new string[]
                    {
                        ".pdf",
                        ".doc",
                        ".docx",
                        ".xls",
                        ".xlsx",
                        ".txt",
                        ".csv",
                        ".wav",
                        ".flac",
                        ".mp3",
                        ".zip",
                        ".rar"
                    },
                AllowedMimeTypes = new string[]
                    {
                        "application/pdf",
                        "application/msword",
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        "application/vnd.ms-excel",
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "text/plain",
                        "text/csv",
                        "audio/wav",
                        "audio/flac",
                        "audio/mpeg",
                        "application/zip",
                        "application/x-rar-compressed"
                    }
            };

            // Check file size
            if (fileSizeBytes > rule.MaxSizeBytes)
                return false;

            // Check file extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!rule.AllowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            //if (!rule.AllowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
            //    return false;

            return true;
        }
        private int ConvertDaysToSeconds(int days)
        {
            return days * 86400; // 24 hours * 60 minutes * 60 seconds
        }
    }
}
