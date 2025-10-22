using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.Details;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.ListItems;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.AssignDMCAAccusationToStaff;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusation;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.ListItems;
using ModerationService.BusinessLogic.DTOs.Snippet;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Helpers.FileHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;
using ModerationService.Common.AppConfigurations.FilePath.interfaces;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.DataAccess.Repositories.interfaces;
using ModerationService.Infrastructure.Models.Kafka;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (roleId == 3)
            {
                query = query.Where(pbrrs => pbrrs.AssignedStaff == staffId).ToList();
            }
            var dmcaAccusation = (await Task.WhenAll(query.Select(async pbrrs =>
            {
                JObject? episode = null;
                if (pbrrs.PodcastEpisodeId.HasValue)
                {
                    episode = await GetPodcastShow(pbrrs.PodcastEpisodeId.Value);
                }
                JObject? show = null;
                if (pbrrs.PodcastShowId.HasValue)
                {
                    show = await GetPodcastShow(pbrrs.PodcastShowId.Value);
                }
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
                            Id = show.Value<Guid>("Id"),
                            Name = show.Value<string>("Name")!,
                            MainImageFileKey = show.Value<string>("MainImageFileKey")!
                        }
                        : null,
                    PodcastEpisode = episode != null
                        ? new PodcastEpisodeSnippetDTO()
                        {
                            Id = episode.Value<Guid>("Id"),
                            Name = episode.Value<string>("Name")!,
                            MainImageFileKey = episode.Value<string>("MainImageFileKey")!
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
                        DmcaAccusationStatusId = 1,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(newDMCAAccusationStatusTracking);

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

            if (roleId == 3)
            {
                query = query.Where(pbrrs => pbrrs.AssignedStaff == accountId);
            }

            // Materialize the entity first, then use async/await for further processing
            var da = await query.FirstOrDefaultAsync();
            if (da == null)
                return null;

            JObject? episode = null;
            if (da.PodcastEpisodeId.HasValue)
            {
                episode = await GetPodcastShow(da.PodcastEpisodeId.Value);
            }
            JObject? show = null;
            if (da.PodcastShowId.HasValue)
            {
                show = await GetPodcastShow(da.PodcastShowId.Value);
            }
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
                        Id = show.Value<Guid>("Id"),
                        Name = show.Value<string>("Name")!,
                        MainImageFileKey = show.Value<string>("MainImageFileKey")!
                    }
                    : null,
                PodcastEpisode = episode != null
                    ? new PodcastEpisodeSnippetDTO()
                    {
                        Id = episode.Value<Guid>("Id"),
                        Name = episode.Value<string>("Name")!,
                        MainImageFileKey = episode.Value<string>("MainImageFileKey")!
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
        public async Task<JObject?> GetPodcastShow(Guid podcastShowId)
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
                                }
                            }),
                            Fields = new[] { "Id", "Name", "MainImageFileKey" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.First as JObject
                : null;
        }
        public async Task<JObject?> GetPodcastEpisode(Guid podcastEpisodeId)
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
                                }
                            }),
                            Fields = new[] { "Id", "Name", "MainImageFileKey" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastEpisode"] is JArray podcastEpisodeArray && podcastEpisodeArray.Count > 0
                ? podcastEpisodeArray.First as JObject
                : null;
        }
        public async Task<DmcaaccusationStatusTracking> CreateDMCAAccusationStatusTracking(DmcaaccusationStatusTracking status)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await _dmcaAccusationStatusTrackingGenericRepository.CreateAsync(status);
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
