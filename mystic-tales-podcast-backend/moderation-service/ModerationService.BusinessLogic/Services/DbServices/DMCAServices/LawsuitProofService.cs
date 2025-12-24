using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.LawsuitProof.Details;
using ModerationService.BusinessLogic.DTOs.LawsuitProof.ListItems;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateLawsuitProof;
using ModerationService.BusinessLogic.Enums.DMCA;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Helpers.FileHelpers;
using ModerationService.BusinessLogic.Models.Mail;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
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
    public class LawsuitProofService
    {
        private readonly IGenericRepository<LawsuitProof> _lawsuitProofGenericRepository;
        private readonly IGenericRepository<LawsuitProofAttachFile> _lawsuitProofAttachFileGenericRepository;
        private readonly IGenericRepository<Dmcaaccusation> _dmcaAccusationGenericRepository;

        private readonly AccountCachingService _accountCachingService;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;

        private readonly ILogger<LawsuitProofService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public LawsuitProofService(
            IGenericRepository<LawsuitProof> lawsuitProofGenericRepository,
            IGenericRepository<LawsuitProofAttachFile> lawsuitProofAttachFileGenericRepository,
            IGenericRepository<Dmcaaccusation> dmcaAccusationGenericRepository,
            AccountCachingService accountCachingService,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            ILogger<LawsuitProofService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper
            )
        {
            _lawsuitProofGenericRepository = lawsuitProofGenericRepository;
            _lawsuitProofAttachFileGenericRepository = lawsuitProofAttachFileGenericRepository;
            _dmcaAccusationGenericRepository = dmcaAccusationGenericRepository;
            _accountCachingService = accountCachingService;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<LawsuitProof> CreateLawsuitProofAsync(LawsuitProof lawsuitProof)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _lawsuitProofGenericRepository.CreateAsync(lawsuitProof);
                    //await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating LawsuitProof");
                    throw;
                }
            //}
        }
        public async Task<LawsuitProofAttachFile> CreateLawsuitProofAttachFileAsync(LawsuitProofAttachFile lawsuitProofAttachFile)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _lawsuitProofAttachFileGenericRepository.CreateAsync(lawsuitProofAttachFile);
                    //await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating LawsuitProofAttachFile");
                    throw;
                }
            //}
        }
        public async Task<LawsuitProofDetailResponseDTO?> GetLawsuitProofByDMCAAccusationId(int dmcaAccusationId)
        {
            var lawsuitProof = _lawsuitProofGenericRepository.FindAll()
                .Where(lp => lp.DmcaAccusationId == dmcaAccusationId)
                .FirstOrDefault();
            if (lawsuitProof == null)
            {
                return null;
            }
            var lawsuitProofAttachFile = await _lawsuitProofAttachFileGenericRepository.FindAll()
                .Where(lpaf => lpaf.LawsuitProofId.Equals(lawsuitProof.Id))
                .Select(lpaf => new LawsuitProofAttachFileListItemResponseDTO
                {
                    Id = lpaf.Id,
                    AttachFileKey = lpaf.AttachFileKey,
                    CreatedAt = lpaf.CreatedAt,
                })
                .ToListAsync();
            AccountStatusCache? staffAccount = null;
            if (lawsuitProof.ValidatedBy != null)
            {
                staffAccount = await _accountCachingService.GetAccountStatusCacheById(lawsuitProof.ValidatedBy.Value);
            }
            return new LawsuitProofDetailResponseDTO
            {
                Id = lawsuitProof.Id,
                DMCAAccusationId = lawsuitProof.DmcaAccusationId,
                IsValid = lawsuitProof.IsValid,
                InValidReason = lawsuitProof.InValidReason,
                ValidatedBy = staffAccount != null ? staffAccount.FullName : null,
                ValidatedAt = lawsuitProof.ValidatedAt,
                CreatedAt = lawsuitProof.CreatedAt,
                UpdatedAt = lawsuitProof.UpdatedAt,
                LawsuitProofAttachFileList = lawsuitProofAttachFile
            };
        }
        public async Task<LawsuitProof> UpdateLawsuitProof(LawsuitProof lawsuitProof)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _lawsuitProofGenericRepository.UpdateAsync(lawsuitProof.Id, lawsuitProof);
                    //await transaction.CommitAsync();
                    _logger.LogInformation("Updated DMCA Lawsuit Proof with ID: {LawsuitProofId}", result.Id);
                    return result;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating LawsuitProof");
                    return null;
                }
            //}
        }
        public async Task CreateLawsuitProofAsync(CreateLawsuitProofParameterDTO parameter, SagaCommandMessage command)
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


                    var dmcaAccusation = await _dmcaAccusationGenericRepository.FindByIdAsync(parameter.DMCAAccusationId,
                        includeFunc: function => function
                        .Include(da => da.DmcaaccusationStatusTrackings));

                    if (dmcaAccusation == null)
                    {
                        throw new Exception("DMCA Accusation not found");
                    }
                    if (dmcaAccusation.DmcaaccusationStatusTrackings.OrderByDescending(dast => dast.CreatedAt).FirstOrDefault().DmcaAccusationStatusId != (int)DMCAAccusationStatusEnum.ValidCounterNotice || dmcaAccusation.ResolvedAt != null)
                    {
                        throw new Exception("DMCA Accusation is not allegible for creating lawsuit proof");
                    }

                    var existingLawsuitProof = _lawsuitProofGenericRepository.FindAll()
                        .Where(lp => lp.DmcaAccusationId == parameter.DMCAAccusationId)
                        .FirstOrDefault();

                    if (existingLawsuitProof != null)
                    {
                        throw new Exception("Lawsuit proof already exists for this DMCA accusation");
                    }

                    var newLawsuitProof = new LawsuitProof
                    {
                        DmcaAccusationId = parameter.DMCAAccusationId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };

                    var createdLawsuitProof = await _lawsuitProofGenericRepository.CreateAsync(newLawsuitProof);

                    var createLawsuitProofAttachFile = new List<string>();

                    if(parameter.LawsuitProofAttachFileKeys == null || parameter.LawsuitProofAttachFileKeys.Count == 0)
                    {
                        throw new Exception("At least one attach file is required for lawsuit proof");
                    }

                    // Process each file
                    foreach (var attachFileKey in parameter.LawsuitProofAttachFileKeys)
                    {

                        var newLawsuitProofAttachFile = new LawsuitProofAttachFile()
                        {
                            LawsuitProofId = createdLawsuitProof.Id,
                            AttachFileKey = "",
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        };

                        var createdLawsuitProofAttachFile = await _lawsuitProofAttachFileGenericRepository.CreateAsync(newLawsuitProofAttachFile);

                        var folderPath = _filePathConfig.DMCA_ACCUSATION_FILE_PATH + "\\" + createdLawsuitProof.DmcaAccusationId;
                        if (attachFileKey != null && attachFileKey != "")
                        {
                            var newAttachFileKey = FilePathHelper.CombinePaths(folderPath, $"{createdLawsuitProofAttachFile.Id}_lawsuit_document{FilePathHelper.GetExtension(attachFileKey)}");
                            await _fileIOHelper.CopyFileToFileAsync(attachFileKey, newAttachFileKey);
                            processedFiles.Add(newAttachFileKey);
                            await _fileIOHelper.DeleteFileAsync(attachFileKey);
                            createdLawsuitProofAttachFile.AttachFileKey = newAttachFileKey;
                            await _lawsuitProofAttachFileGenericRepository.UpdateAsync(createdLawsuitProofAttachFile.Id, createdLawsuitProofAttachFile);
                        }

                        createLawsuitProofAttachFile.Add(createdLawsuitProofAttachFile.AttachFileKey);
                    }

                    //var newDmcaStatusTracking = new DmcaaccusationStatusTracking
                    //{
                    //    DmcaAccusationId = createdLawsuitProof.DmcaAccusationId,
                    //    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.LawsuitFiled,
                    //    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    //};

                    //var createDmcaStatusTracking = await _dmcaAccusationService.CreateDMCAAccusationStatusTracking(newDmcaStatusTracking);
                    //if (createDmcaStatusTracking == null)
                    //{
                    //    throw new Exception("Failed to created dmca status tracking");
                    //}

                    var accuserMailSendingRequestData = JObject.FromObject(new
                    {
                        SendModerationServiceEmailInfo = new
                        {
                            MailTypeName = "DMCALawsuitProofPending",
                            ToEmail = dmcaAccusation.AccuserEmail,
                            MailObject = new DMCALawsuitProofPendingMailViewModel
                            {
                                AccuserEmail = dmcaAccusation.AccuserEmail,
                                AccuserFullName = dmcaAccusation.AccuserFullName,
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
                        { "DMCAAccusationId", createdLawsuitProof.DmcaAccusationId },
                        { "LawsuitProofId", createdLawsuitProof.Id },
                        //{ "LawsuitProofAttachFileKeys", JArray.FromObject(createLawsuitProofAttachFile) },
                        { "CreatedAt", createdLawsuitProof.CreatedAt }
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
                    _logger.LogInformation("Successfully create lawsuit proof for SagaId: {SagaId}", sagaId);
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

                    foreach (var attachFile in parameter.LawsuitProofAttachFileKeys)
                    {
                        if (attachFile != null && attachFile != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(attachFile);
                        }
                    }
                    _logger.LogError(ex, "Error occurred while creating lawsuit proof for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create lawsuit proof failed, error: " + ex.Message }
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
                    _logger.LogInformation("Create lawsuit proof failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
    }
}
