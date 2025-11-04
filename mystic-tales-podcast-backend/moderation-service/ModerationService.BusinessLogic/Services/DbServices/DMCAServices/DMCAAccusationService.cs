using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.Details;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.ListItems;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.AssignDMCAAccusationToStaff;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CancelDMCAAccusationReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusation;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusationReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.UpdateDMCAAccusationStatus;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.ValidateDMCAAccusationReport;
using ModerationService.BusinessLogic.DTOs.Podcast;
using ModerationService.BusinessLogic.DTOs.Snippet;
using ModerationService.BusinessLogic.Enums.Account;
using ModerationService.BusinessLogic.Enums.DMCA;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Enums.Podcast;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Helpers.FileHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
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
using SystemConfigurationService.DataAccess.Entities.SqlServer;

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
                var episode = await GetPodcastEpisode(pbrrs.PodcastEpisodeId.Value);
                
                var show = await GetPodcastShow(pbrrs.PodcastShowId.Value);
                AccountStatusCache? staff = null;
                if (pbrrs.AssignedStaff.HasValue)
                {
                    staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff.Value);
                }
                return new DMCAAccusationListItemResponseDTO()
                {
                    Id = pbrrs.Id,
                    AccuserEmail = pbrrs.AccuserEmail,
                    AccuserPhone = pbrrs.AccuserPhone,
                    AccuserFullName = pbrrs.AccuserFullName,
                    PodcastShow = show != null
                        ? new PodcastShowSnippetDTO()
                        {
                            Id = show.Id,
                            Name = show.Name,
                            MainImageFileKey = show.MainImageFileKey
                        }
                        : null,
                    PodcastEpisode = episode != null
                        ? new PodcastEpisodeSnippetDTO()
                        {
                            Id = episode.Id,
                            Name = episode.Name,
                            MainImageFileKey = episode.MainImageFileKey
                        }
                        : null,
                    AssignedStaff = staff != null
                        ? new AssignedStaffSnippetDTO()
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

                    // Process each file
                    foreach (var attachFileKey in parameter.DMCANoticeFileKeys)
                    {

                        var newDMCANoticeAttachFile = new DmcanoticeAttachFile()
                        {
                            DmcaNoticeId = createDmcaNotice.Id,
                            AttachFileKey = null,
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

                    //Send email to B

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
                            { "DMCAAttachFileKeys", JArray.FromObject(createDMCANoticeAttachFile) },
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

                    foreach (var attachFile in parameter.DMCANoticeFileKeys)
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

            var episode = await GetPodcastShow(da.PodcastEpisodeId.Value);

            var show = await GetPodcastShow(da.PodcastShowId.Value);
            AccountStatusCache? staff = null;
            if (da.AssignedStaff.HasValue)
            {
                staff = await _accountCachingService.GetAccountStatusCacheById(da.AssignedStaff.Value);
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
                PodcastShow = show != null
                    ? new PodcastShowSnippetDTO()
                    {
                        Id = show.Id,
                        Name = show.Name,
                        MainImageFileKey = show.MainImageFileKey
                    }
                    : null,
                PodcastEpisode = episode != null
                    ? new PodcastEpisodeSnippetDTO()
                    {
                        Id = episode.Id,
                        Name = episode.Name,
                        MainImageFileKey = episode.MainImageFileKey
                    }
                    : null,
                AssignedStaff = staff != null
                    ? new AssignedStaffSnippetDTO()
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
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var dmcaAccusation = await _dmcaAccusationGenericRepository.FindByIdWithPaths(parameter.DMCAAccusationId,
                        "DMCAAccusationStatusTracking",
                        "DMCANotice",
                        "CounterNotice",
                        "LawsuitProof");
                    if (dmcaAccusation == null)
                    {
                        throw new Exception($"DMCA Accusation with Id: {parameter.DMCAAccusationId} not found");
                    }
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

                            //Send TakeDownFlow

                            //Send Confirm email to Accuser

                            //Send Email to Accused

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

                            //Send Confirm email to Accuser

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

                            //Send Email to Accused

                            //Send Confirm email to Accuser

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

                    var dmcaAccusationReport = await _dmcaAccusationConclusionReportGenericRepository.FindByIdAsync(parameter.DMCAAccusationReportId);

                    if (parameter.IsValid)
                    {
                        dmcaAccusationReport.IsRejected = true;
                        dmcaAccusationReport.CompletedAt = _dateHelper.GetNowByAppTimeZone();
                        dmcaAccusationReport.UpdatedAt = _dateHelper.GetNowByAppTimeZone();

                        switch(dmcaAccusationReport.DmcaAccusationConclusionReportTypeId)
                        {
                            case (int)DMCAAccusationConclusionReportTypeEnum.InvalidDMCANotice:
                                //Send Email to Accuser
                                //Send Email to Accused
                                //Switch Status
                                var invalidDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.InvalidDMCANotice,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(invalidDmcaAccusationStatusTracking);
                                break;
                            case (int)DMCAAccusationConclusionReportTypeEnum.InvalidCounterNotice:
                                //Send Email to Accuser
                                //Switch Status
                                var invalidCounterNoticeDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.InvalidCounterNotice,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(invalidCounterNoticeDmcaAccusationStatusTracking);
                                break;
                            case (int)DMCAAccusationConclusionReportTypeEnum.InvalidLawsuitProof:
                                //Restore Content
                                //Send Email to Accuser
                                //Switch Status
                                var invalidLawsuitProofDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                                {
                                    DmcaAccusationId = dmcaAccusationReport.DmcaAccusationId,
                                    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.InvalidLawsuitProof,
                                    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                                };
                                await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(invalidLawsuitProofDmcaAccusationStatusTracking);
                                break;
                            case (int)DMCAAccusationConclusionReportTypeEnum.AccuserLawsuitWin:
                                //Send Email to Accuser
                                //Send Email to Accused
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
                                //Send Email to Accuser
                                //Send Email to Accused
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
                        dmcaAccusationReport.IsRejected = false;
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
                    dmcaAccusationConclusionReport.CancelledAt = _dateHelper.GetNowByAppTimeZone();
                    await _dmcaAccusationConclusionReportGenericRepository.UpdateAsync(dmcaAccusationConclusionReport.Id, dmcaAccusationConclusionReport);

                    var newResponseData = command.RequestData;
                    newResponseData["CancelledAt"] = dmcaAccusationConclusionReport.CancelledAt;
                    var newMessageName = messageName = ".success";
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
                                include = "PodcastEpisodeStatusTracking"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastEpisode"] is JArray podcastEpisodeArray && podcastEpisodeArray.Count > 0
                ? podcastEpisodeArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<PodcastEpisodeDTO>() : null;
        }
        public async Task<PodcastChannelDTO?> GetPodcastChannel(Guid podcastChannelId)
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
                                include = "PodcastChannelStatusTracking"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastChannel"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                ? podcastChannelArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<PodcastChannelDTO>() : null;
        }
        public async Task<PodcastShowDTO?> GetPodcastShow(Guid podcastShowId)
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
                                include = "PodcastShowStatusTracking"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<PodcastShowDTO>() : null;
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

            var realResult = result.Results?["activeSystemConfigProfile"] is JArray configArray && configArray.Count > 0
                ? configArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<SystemConfigProfileDTO>() : null;
        }
        //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //{
        //    try
        //    {
        //        var messageName = command.MessageName;
        //        var sagaId = command.SagaInstanceId;
        //        var flowName = command.FlowName;
        //        var responseData = command.LastStepResponseData;

        //await transaction.CommitAsync();

        //var newResponseData = new JObject
        //            {
        //                { "AccountId", registrationResult.AccountId },
        //                { "PodcastSubscriptionRegistrationId", registrationResult.Id },
        //                { "CancelledAt", registrationResult.CancelledAt }
        //            };
        //var newMessageName = messageName + ".success";
        //var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //    topic: KafkaTopicEnum.PaymentProcessingDomain,
        //    requestData: command.RequestData,
        //    responseData: newResponseData,
        //    sagaInstanceId: sagaId,
        //    flowName: flowName,
        //    messageName: newMessageName);
        //await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
        //_logger.LogInformation("Successfully cancel podcast subscription registration for SagaId: {SagaId}", sagaId);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Error occurred while create podcast buddy report for SagaId: {SagaId}", command.SagaInstanceId);
        //        var newResponseData = new JObject{
        //            { "ErrorMessage", "Create podcast buddy report failed, error: " + ex.Message }
        //        };
        //        var newMessageName = command.MessageName + ".failed";
        //        var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //            topic: KafkaTopicEnum.PaymentProcessingDomain,
        //            requestData: command.RequestData,
        //            responseData: newResponseData,
        //            sagaInstanceId: command.SagaInstanceId,
        //            flowName: command.FlowName,
        //            messageName: newMessageName);
        //        await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
        //        _logger.LogInformation("Create podcast buddy report failed for SagaId: {SagaId}", command.SagaInstanceId);
        //    }
        //}
    }
}
