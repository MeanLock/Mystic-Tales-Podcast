using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.CounterNotice.Details;
using ModerationService.BusinessLogic.DTOs.CounterNotice.ListItems;
using ModerationService.BusinessLogic.DTOs.DMCANotice.Details;
using ModerationService.BusinessLogic.DTOs.DMCANotice.ListItems;
using ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateCounterNotice;
using ModerationService.BusinessLogic.DTOs.Podcast;
using ModerationService.BusinessLogic.Enums.DMCA;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Helpers.FileHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
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
    public class CounterNoticeService
    {
        private readonly IGenericRepository<CounterNotice> _counterNoticeGenericRepository;
        private readonly IGenericRepository<CounterNoticeAttachFile> _counterNoticeAttachFileGenericRepository;
        private readonly IGenericRepository<Dmcaaccusation> _dmcaAccusationGenericRepository;

        private readonly AccountCachingService _accountCachingService;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;

        private readonly ILogger<CounterNoticeService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public CounterNoticeService(
            IGenericRepository<CounterNotice> counterNoticeGenericRepository,
            IGenericRepository<CounterNoticeAttachFile> counterNoticeAttachFileGenericRepository,
            IGenericRepository<Dmcaaccusation> dmcaAccusationGenericRepository,
            AccountCachingService accountCachingService,
            IFilePathConfig filePathConfig,
            FileIOHelper fileIOHelper,
            ILogger<CounterNoticeService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper
            )
        {
            _counterNoticeGenericRepository = counterNoticeGenericRepository;
            _counterNoticeAttachFileGenericRepository = counterNoticeAttachFileGenericRepository;
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
        public async Task<CounterNoticeDetailResponseDTO?> GetCounterNoticeByDMCAAccusationId(int dmcaAccusationId)
        {
            var counterNotice = _counterNoticeGenericRepository.FindAll()
                .Where(lp => lp.DmcaAccusationId == dmcaAccusationId)
                .FirstOrDefault();
            if(counterNotice == null)
            {
                return null;
            }
            var counterNoticeAttachFile = await _counterNoticeAttachFileGenericRepository.FindAll()
                .Where(lpaf => lpaf.CounterNoticeId.Equals(counterNotice.Id))
                .Select(lpaf => new CounterNoticeAttachFileListItemResponseDTO
                {
                    Id = lpaf.Id,
                    AttachFileKey = lpaf.AttachFileKey,
                    CreatedAt = lpaf.CreatedAt,
                })
                .ToListAsync();
            AccountStatusCache? staffAccount = null;
            if (counterNotice.ValidatedBy != null)
            {
                staffAccount = await _accountCachingService.GetAccountStatusCacheById(counterNotice.ValidatedBy.Value);
            }
            return new CounterNoticeDetailResponseDTO
            {
                Id = counterNotice.Id,
                DMCAAccusationId = counterNotice.DmcaAccusationId,
                IsValid = counterNotice.IsValid,
                InValidReason = counterNotice.InvalidReason,
                ValidatedBy = staffAccount != null ? staffAccount.FullName : null,
                ValidatedAt = counterNotice.ValidatedAt,
                CreatedAt = counterNotice.CreatedAt,
                UpdatedAt = counterNotice.UpdatedAt,
                CounterNoticeAttachFileList = counterNoticeAttachFile
            };
        }
        public async Task<CounterNotice> UpdateCounterNotice(CounterNotice counterNotice)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _counterNoticeGenericRepository.UpdateAsync(counterNotice.Id, counterNotice);
                    //await transaction.CommitAsync();
                    _logger.LogInformation("Updated DMCA Counter Notice with ID: {CounterNoticeId}", result.Id);
                    return result;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating CounterNotice");
                    throw;
                }
            //}
        }
        public async Task CreateCounterNoticeAsync(CreateCounterNoticeParameterDTO parameter, SagaCommandMessage command)
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

                    if(dmcaAccusation == null)
                    {
                        throw new Exception("DMCA Accusation not found");
                    }
                    if(dmcaAccusation.DmcaaccusationStatusTrackings.OrderByDescending(dast => dast.CreatedAt).FirstOrDefault().DmcaAccusationStatusId != (int)DMCAAccusationStatusEnum.ValidDMCANotice || dmcaAccusation.ResolvedAt != null)
                    {
                        throw new Exception("DMCA Accusation is not allegible for creating counter notice");
                    }
                    
                    var existingCounterNotice = _counterNoticeGenericRepository.FindAll()
                        .Where(cn => cn.DmcaAccusationId == parameter.DMCAAccusationId)
                        .FirstOrDefault();

                    if (existingCounterNotice != null)
                    {
                        throw new Exception("Counter notice already exists for this DMCA accusation");
                    }

                    var newCounterNotice = new CounterNotice
                    {
                        DmcaAccusationId = parameter.DMCAAccusationId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };

                    var createdCounterNotice = await _counterNoticeGenericRepository.CreateAsync(newCounterNotice);

                    var createCounterNoticeAttachFile = new List<string>();

                    if(parameter.CounterNoticeAttachFileKeys == null || parameter.CounterNoticeAttachFileKeys.Count == 0)
                    {
                        throw new Exception("No attach files provided");
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

                    // Process each file
                    foreach (var attachFileKey in parameter.CounterNoticeAttachFileKeys)
                    {

                        var newCounterNoticeAttachFile = new CounterNoticeAttachFile()
                        {
                            CounterNoticeId = createdCounterNotice.Id,
                            AttachFileKey = "",
                            CreatedAt = _dateHelper.GetNowByAppTimeZone()
                        };

                        var counterNoticeAttachFile = await _counterNoticeAttachFileGenericRepository.CreateAsync(newCounterNoticeAttachFile);

                        var folderPath = _filePathConfig.DMCA_ACCUSATION_FILE_PATH + "\\" + createdCounterNotice.DmcaAccusationId;
                        if (attachFileKey != null && attachFileKey != "")
                        {
                            var newAttachFileKey = FilePathHelper.CombinePaths(folderPath, $"{counterNoticeAttachFile.Id}_counter_notice{FilePathHelper.GetExtension(attachFileKey)}");
                            await _fileIOHelper.CopyFileToFileAsync(attachFileKey, newAttachFileKey);
                            processedFiles.Add(newAttachFileKey);
                            await _fileIOHelper.DeleteFileAsync(attachFileKey);
                            newCounterNoticeAttachFile.AttachFileKey = newAttachFileKey;
                            await _counterNoticeAttachFileGenericRepository.UpdateAsync(newCounterNoticeAttachFile.Id, newCounterNoticeAttachFile);
                        }

                        createCounterNoticeAttachFile.Add(counterNoticeAttachFile.AttachFileKey);
                    }

                    //var newDmcaStatusTracking = new DmcaaccusationStatusTracking
                    //{
                    //    DmcaAccusationId = createdCounterNotice.DmcaAccusationId,
                    //    DmcaAccusationStatusId = (int)DMCAAccusationStatusEnum.CounterReviewing,
                    //    CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    //};

                    //var createDmcaStatusTracking = await _dmcaAccusationService.CreateDMCAAccusationStatusTracking(newDmcaStatusTracking);
                    //if(createDmcaStatusTracking == null)
                    //{
                    //    throw new Exception("Failed to created dmca status tracking");
                    //}

                    //Send confirm email to accused
                    var accuserMailSendingRequestData = JObject.FromObject(new
                    {
                        SendModerationServiceEmailInfo = new
                        {
                            MailTypeName = "DMCACounterNoticePending",
                            ToEmail = podcaster.Email,
                            MailObject = new DMCACounterNoticePendingMailViewModel
                            {
                                PodcasterEmail = podcaster.Email,
                                PodcasterFullName = podcaster.FullName,
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
                        { "DMCAAccusationId", createdCounterNotice.DmcaAccusationId },
                        { "DMCAcounternoticeId", createdCounterNotice.Id },
                        //{ "CounterNoticeAttachFileKeys", JArray.FromObject(createCounterNoticeAttachFile) },
                        { "CreatedAt", createdCounterNotice.CreatedAt }
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
                    _logger.LogInformation("Successfully create counter notice for SagaId: {SagaId}", sagaId);
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

                    foreach (var attachFile in parameter.CounterNoticeAttachFileKeys)
                    {
                        if (attachFile != null && attachFile != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(attachFile);
                        }
                    }
                    _logger.LogError(ex, "Error occurred while creating counter notice for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create counter notice failed, error: " + ex.Message }
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
                    _logger.LogInformation("Create counter notice failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
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
    }
}
