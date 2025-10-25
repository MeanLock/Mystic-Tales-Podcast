using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.Details;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.ListItems;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.AssignDMCAAccusationToStaff;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusation;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.UpdateDMCAAccusationStatus;
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
                    LastLawsuitCheckingAlertAt = pbrrs.LastLawsuitCheckingAlertAt,
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
                        PodcastShowId = parameter.PodcastShowId,
                        PodcastEpisodeId = parameter.PodcastEpisodeId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    var createdDmcaAccusation = await _dmcaAccusationGenericRepository.CreateAsync(newDmcaAccusation);

                    var newDmcaNotice = new Dmcanotice
                    {
                        PodcastShowId = parameter.PodcastShowId,
                        PodcastEpisodeId = parameter.PodcastEpisodeId,
                        AccountId = parameter.AccountId,
                        AccountEmail = parameter.AccuserEmail,
                        AccountPhone = parameter.AccuserPhone,
                        GoodFaithStatement = parameter.GoodFaithStatement,
                        WorkClaimed = parameter.WorkClaimed,
                        Signature = parameter.Signature,
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
                        DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.Pending,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(newDMCAAccusationStatusTracking);

                    //Send email to B

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                        {
                            { "DMCAAccusationId", createdDmcaAccusation.Id },
                            { "DMCANoticeId", createDmcaNotice.Id },
                            { "AccountId", createDmcaNotice.AccountId },
                            { "AccountEmail", createDmcaNotice.AccountEmail },
                            { "AccountPhone", createDmcaNotice.AccountPhone },
                            { "PodcastShowId", createdDmcaAccusation.PodcastShowId },
                            { "PodcastEpisodeId", createdDmcaAccusation.PodcastEpisodeId },
                            { "GoodFaithStatement", createDmcaNotice.GoodFaithStatement },
                            { "WorkClaimed", createDmcaNotice.WorkClaimed },
                            { "Signature", createDmcaNotice.Signature },
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
                LastLawsuitCheckingAlertAt = da.LastLawsuitCheckingAlertAt,
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

                    var newDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                    {
                        DmcaAccusationId = createdDmcaAccusation.Id,
                        DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.Reviewing,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(newDmcaAccusationStatusTracking);

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
                    if(dmcaAccusation == null)
                    {
                        throw new Exception($"DMCA Accusation with Id: {parameter.DMCAAccusationId} not found");
                    }
                    var accusedId = 0;
                    switch (parameter.DMCAAccusationAction)
                    {
                        case (int)DMCAAccusationQueryEnum.TAKEDOWN_ACTIVE:
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
                            if (currentStatus1 != (int)DMCAAccusationStatusEnum.Reviewing)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation with Reviewing status can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send Confirm email to Accuser

                            //Send Email to Accused

                            //Switch Status
                            var takedownDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.TakeDownPermanent,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(takedownDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                            break;
                        case (int)DMCAAccusationQueryEnum.REJECTED:
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
                            if (currentStatus2 != (int)DMCAAccusationStatusEnum.Reviewing)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation with Reviewing status can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send Confirm email to Accuser

                            //Punish Accuser (B -10 point)
                            var accuserPunishmentRequestData1 = new JObject
                            {
                                { "AccountId", dmcaAccusation.Dmcanotices.First().AccountId },
                                { "ViolationPoint", 10 }
                            };
                            var accountPunishmentStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.UserManagementDomain,
                                requestData: accuserPunishmentRequestData1,
                                sagaInstanceId: null,
                                messageName: "user-violation-punishment-flow");
                            var accountPunishmentResult1 = await _messagingService.SendSagaMessageAsync(accountPunishmentStartSagaTriggerMessage1);
                            _logger.LogInformation($"Send user-violation-punishment-flow with Saga Id: {accountPunishmentStartSagaTriggerMessage1.SagaInstanceId}");

                            //Switch Status
                            var rejectDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.Rejected,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(rejectDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                            break;
                        case (int)DMCAAccusationQueryEnum.CLOSED_WITHDRAWN:
                            //Validate Accuser Account
                            if (dmcaAccusation.Dmcanotices.First().AccountId != parameter.AccountId)
                            {
                                throw new Exception($"The logged in account Id: {parameter.AccountId} is not authorized to perform this action this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                            }

                            //Validate DMCA Accusation status
                            var currentStatus3 = dmcaAccusation.DmcaaccusationStatusTrackings
                                .OrderByDescending(dast => dast.CreatedAt)
                                .First()
                                .DmcaAccusationStatusId;
                            if (currentStatus3 >= (int)DMCAAccusationStatusEnum.LawsuitFiled)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation that havent file a lawsuit can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send Email to Accused

                            //Send Confirm email to Accuser

                            //Switch Status
                            var withdrawDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.CloseWithdrawn,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(withdrawDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                            break;
                        case (int)DMCAAccusationQueryEnum.TAKEDOWN_PERMANENT:
                            //Validate Accused Account
                            if (dmcaAccusation.PodcastShowId != null)
                            {
                                var show = await GetPodcastShow(dmcaAccusation.PodcastShowId.Value);
                                accusedId = show.PodcasterId;
                                if (accusedId != parameter.AccountId)
                                {
                                    throw new Exception($"The logged in account Id: {parameter.AccountId} is not authorized to perform this action this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                                }
                            }
                            if (dmcaAccusation.PodcastEpisodeId != null)
                            {
                                var episode = await GetPodcastShow(dmcaAccusation.PodcastEpisodeId.Value);
                                accusedId = episode.PodcasterId;
                                if (accusedId != parameter.AccountId)
                                {
                                    throw new Exception($"The logged in account Id: {parameter.AccountId} is not authorized to perform this action this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                                }
                            }

                            //Validate DMCA Accusation status
                            var currentStatus4 = dmcaAccusation.DmcaaccusationStatusTrackings
                                .OrderByDescending(dast => dast.CreatedAt)
                                .First()
                                .DmcaAccusationStatusId;
                            if (currentStatus4 >= (int)DMCAAccusationStatusEnum.LawsuitFiled)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation that havent file a lawsuit can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send Confirm email to Accused

                            //Send Email to Accuser

                            //Remove Content
                            if (dmcaAccusation.PodcastShowId != null)
                            {
                                var removeShowRequestData1 = new JObject
                                {
                                    { "PodcastShowId", dmcaAccusation.PodcastShowId.Value }
                                };
                                var showRemoveStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ContentManagementDomain,
                                    requestData: removeShowRequestData1,
                                    sagaInstanceId: null,
                                    messageName: "remove-show-flow");
                                var showRemoveResult1 = await _messagingService.SendSagaMessageAsync(showRemoveStartSagaTriggerMessage1);
                                _logger.LogInformation($"Send remove-show-flow with Saga Id: {showRemoveStartSagaTriggerMessage1.SagaInstanceId}");
                            } else if (dmcaAccusation.PodcastEpisodeId != null)
                            {
                                //Take down Episode
                                var removeEpisodeRequestData1 = new JObject
                                {
                                    { "PodcastEpisodeId", dmcaAccusation.PodcastEpisodeId.Value }
                                };
                                var episodeRemoveStartSagaTriggerMessage1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ContentManagementDomain,
                                    requestData: removeEpisodeRequestData1,
                                    sagaInstanceId: null,
                                    messageName: "remove-episode-flow");
                                var episodeRemoveResult1 = await _messagingService.SendSagaMessageAsync(episodeRemoveStartSagaTriggerMessage1);
                                _logger.LogInformation($"Send remove-episode-flow with Saga Id: {episodeRemoveStartSagaTriggerMessage1.SagaInstanceId}");
                            };

                            //Punish Accused (A -200 point)
                            var accuserPunishmentRequestData2 = new JObject
                            {
                                { "AccountId", accusedId },
                                { "ViolationPoint", 200 }
                            };
                            var accountPunishmentStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.UserManagementDomain,
                                requestData: accuserPunishmentRequestData2,
                                sagaInstanceId: null,
                                messageName: "user-violation-punishment-flow");
                            var accountPunishmentResult2 = await _messagingService.SendSagaMessageAsync(accountPunishmentStartSagaTriggerMessage2);
                            _logger.LogInformation($"Send user-violation-punishment-flow with Saga Id: {accountPunishmentStartSagaTriggerMessage2.SagaInstanceId}");

                            //Switch Status
                            var permanentTakeDownDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.TakeDownPermanent,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(permanentTakeDownDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                            break;
                        case (int)DMCAAccusationQueryEnum.COUNTER_ACCEPTED:
                            //Validate Staff Account
                            if (dmcaAccusation.AssignedStaff != parameter.AccountId)
                            {
                                throw new Exception($"The logged in staff Id: {parameter.AccountId} is not authorized to update this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                            }

                            //Validate DMCA Accusation status
                            var currentStatus5 = dmcaAccusation.DmcaaccusationStatusTrackings
                                .OrderByDescending(dast => dast.CreatedAt)
                                .First()
                                .DmcaAccusationStatusId;
                            if (currentStatus5 != (int)DMCAAccusationStatusEnum.CounterReviewing)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation that has counter notice in reviewing can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send email to Accuser

                            //Switch Status
                            var counterAcceptedDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.LawsuitPending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(counterAcceptedDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                            break;
                        case (int)DMCAAccusationQueryEnum.COUNTER_REJECTED:
                            //Validate Staff Account
                            if (dmcaAccusation.AssignedStaff != parameter.AccountId)
                            {
                                throw new Exception($"The logged in staff Id: {parameter.AccountId} is not authorized to update this DMCA Accusation with Id: {parameter.DMCAAccusationId}");
                            }

                            //Validate DMCA Accusation status
                            var currentStatus6 = dmcaAccusation.DmcaaccusationStatusTrackings
                                .OrderByDescending(dast => dast.CreatedAt)
                                .First()
                                .DmcaAccusationStatusId;
                            if (currentStatus6 != (int)DMCAAccusationStatusEnum.CounterReviewing)
                            {
                                throw new Exception($"DMCAAccusation with Id: {dmcaAccusation.Id}. Only DMCA Accusation that has counter notice in reviewing can perform this action: {Enum.GetName(typeof(DMCAAccusationQueryEnum), parameter.DMCAAccusationAction)}");
                            }

                            //Send email to Accused

                            //Send email to Accuser

                            if(dmcaAccusation.PodcastShowId != null)
                            {
                                var show = await GetPodcastShow(dmcaAccusation.PodcastShowId.Value);
                                accusedId = show.PodcasterId;
                            }
                            if (dmcaAccusation.PodcastEpisodeId != null)
                            {
                                var episode = await GetPodcastShow(dmcaAccusation.PodcastEpisodeId.Value);
                                accusedId = episode.PodcasterId;
                            }

                            //Punish Accused (A -10 point)
                            var accuserPunishmentRequestData3 = new JObject
                            {
                                { "AccountId", accusedId },
                                { "ViolationPoint", 10 }
                            };
                            var accountPunishmentStartSagaTriggerMessage3 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.UserManagementDomain,
                                requestData: accuserPunishmentRequestData3,
                                sagaInstanceId: null,
                                messageName: "user-violation-punishment-flow");
                            var accountPunishmentResult3 = await _messagingService.SendSagaMessageAsync(accountPunishmentStartSagaTriggerMessage3);
                            _logger.LogInformation($"Send user-violation-punishment-flow with Saga Id: {accountPunishmentStartSagaTriggerMessage3.SagaInstanceId}");

                            //Remove Content
                            if (dmcaAccusation.PodcastShowId != null)
                            {
                                var removeShowRequestData2 = new JObject
                                {
                                    { "PodcastShowId", dmcaAccusation.PodcastShowId.Value }
                                };
                                var showRemoveStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ContentManagementDomain,
                                    requestData: removeShowRequestData2,
                                    sagaInstanceId: null,
                                    messageName: "remove-show-flow");
                                var showRemoveResult2 = await _messagingService.SendSagaMessageAsync(showRemoveStartSagaTriggerMessage2);
                                _logger.LogInformation($"Send remove-show-flow with Saga Id: {showRemoveStartSagaTriggerMessage2.SagaInstanceId}");
                            }
                            else if (dmcaAccusation.PodcastEpisodeId != null)
                            {
                                //Take down Episode
                                var removeEpisodeRequestData2 = new JObject
                                {
                                    { "PodcastEpisodeId", dmcaAccusation.PodcastEpisodeId.Value }
                                };
                                var episodeRemoveStartSagaTriggerMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.ContentManagementDomain,
                                    requestData: removeEpisodeRequestData2,
                                    sagaInstanceId: null,
                                    messageName: "remove-episode-flow");
                                var episodeRemoveResult2 = await _messagingService.SendSagaMessageAsync(episodeRemoveStartSagaTriggerMessage2);
                                _logger.LogInformation($"Send remove-episode-flow with Saga Id: {episodeRemoveStartSagaTriggerMessage2.SagaInstanceId}");
                            };

                            //Punish Accused (A -200 point)
                            var accuserPunishmentRequestData4 = new JObject
                            {
                                { "AccountId", accusedId },
                                { "ViolationPoint", 200 }
                            };
                            var accountPunishmentStartSagaTriggerMessage4 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.UserManagementDomain,
                                requestData: accuserPunishmentRequestData4,
                                sagaInstanceId: null,
                                messageName: "user-violation-punishment-flow");
                            var accountPunishmentResult4 = await _messagingService.SendSagaMessageAsync(accountPunishmentStartSagaTriggerMessage4);
                            _logger.LogInformation($"Send user-violation-punishment-flow with Saga Id: {accountPunishmentStartSagaTriggerMessage4.SagaInstanceId}");

                            //Switch Status
                            var counterRejectedDmcaAccusationStatusTracking = new DmcaaccusationStatusTracking
                            {
                                DmcaAccusationId = dmcaAccusation.Id,
                                DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.DMCAWins,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(counterRejectedDmcaAccusationStatusTracking);
                            dmcaAccusation.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _dmcaAccusationGenericRepository.UpdateAsync(dmcaAccusation.Id, dmcaAccusation);
                            break;
                        case (int)DMCAAccusationQueryEnum.LAWSUIT_VERIFIED:

                            break;
                        case (int)DMCAAccusationQueryEnum.LAWSUIT_REJECTED:
                            break;
                        case (int)DMCAAccusationQueryEnum.DMCA_WINS:
                            break;
                        case (int)DMCAAccusationQueryEnum.COUNTER_WINS:
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
