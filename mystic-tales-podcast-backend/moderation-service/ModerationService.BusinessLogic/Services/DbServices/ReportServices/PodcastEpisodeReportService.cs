using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateEpisodeReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReport;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport.Details;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport.ListItems;
using ModerationService.BusinessLogic.DTOs.Snippet;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.DataAccess.Repositories.interfaces;
using ModerationService.Infrastructure.Models.Kafka;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;

namespace ModerationService.BusinessLogic.Services.DbServices.ReportServices
{
    public class PodcastEpisodeReportService
    {
        private readonly IGenericRepository<PodcastEpisodeReport> _podcastEpisodeReportGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeReportReviewSession> _podcastEpisodeReportReviewSessionGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeReportType> _podcastEpisodeReportTypeGenericRepository;

        private readonly AccountCachingService _accountCachingService;

        private readonly ILogger<PodcastEpisodeReportService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public PodcastEpisodeReportService(
            IGenericRepository<PodcastEpisodeReport> podcastEpisodeReportGenericRepository,
            IGenericRepository<PodcastEpisodeReportReviewSession> podcastEpisodeReportReviewSessionGenericRepository,
            IGenericRepository<PodcastEpisodeReportType> podcastEpisodeReportTypeGenericRepository,
            AccountCachingService accountCachingService,
            ILogger<PodcastEpisodeReportService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper
            )
        {
            _podcastEpisodeReportGenericRepository = podcastEpisodeReportGenericRepository;
            _podcastEpisodeReportReviewSessionGenericRepository = podcastEpisodeReportReviewSessionGenericRepository;
            _podcastEpisodeReportTypeGenericRepository = podcastEpisodeReportTypeGenericRepository;
            _accountCachingService = accountCachingService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<List<PodcastEpisodeReportListItemResponseDTO>> GetAllPodcastEpisodeReportAsync()
        {
            var query = await _podcastEpisodeReportGenericRepository.FindAll(
                predicate: null,
                includeFunc: source => source
                    .Include(r => r.PodcastEpisodeReportType)
                ).ToListAsync();
            var podcastEpisodeReport = (await Task.WhenAll(query.Select(async pbr =>
            {
                var Episode = await GetPodcastEpisode(pbr.PodcastEpisodeId);
                return new PodcastEpisodeReportListItemResponseDTO()
                {
                    Id = pbr.Id,
                    Content = pbr.Content,
                    AccountId = pbr.AccountId,
                    PodcastEpisode = new PodcastEpisodeSnippetDTO()
                    {
                        Id = Episode != null && Episode["Id"] != null ? Episode.Value<Guid>("Id") : Guid.Empty,
                        Name = Episode != null && Episode["Name"] != null ? Episode.Value<string>("Name") : "",
                        MainImageFileKey = Episode != null && Episode["MainImageFileKey"] != null ? Episode.Value<string>("MainImageFileKey") : "",
                    },
                    PodcastEpisodeReportType = new PodcastEpisodeReportTypeDTO()
                    {
                        Id = pbr.PodcastEpisodeReportType.Id,
                        Name = pbr.PodcastEpisodeReportType.Name,
                    },
                    ResolvedAt = pbr.ResolvedAt,
                    CreatedAt = pbr.CreatedAt,
                };
            }))).ToList();
            return podcastEpisodeReport;
        }
        public async Task CreatePodcastEpisodeReportAsync(CreateEpisodeReportParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var systemConfig = await GetActiveSystemConfigProfile();

                    var newEpisodeReport = new PodcastEpisodeReport()
                    {
                        AccountId = parameter.AccountId,
                        Content = parameter.Content,
                        PodcastEpisodeId = parameter.PodcastEpisodeId,
                        PodcastEpisodeReportTypeId = parameter.PodcastEpisodeReportTypeId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };

                    var EpisodeReport = await _podcastEpisodeReportGenericRepository.CreateAsync(newEpisodeReport);

                    var existingEpisodeReport = await _podcastEpisodeReportGenericRepository.FindAll()
                        .Where(br => br.PodcastEpisodeId == parameter.PodcastEpisodeId && br.ResolvedAt == null)
                        .ToListAsync();
                    if (existingEpisodeReport.Count() >= systemConfig["ReviewSessionConfig"].Value<int>("PodcastEpisodeUnResolvedReportStreak"))
                    {
                        var staffList = await GetStaffList();
                        var randomStaff = GetRandomItemFromJArray(staffList);
                        var staffId = randomStaff["Id"]?.Value<int>();

                        var newEpisodeReportReviewSession = new PodcastEpisodeReportReviewSession()
                        {
                            AssignedStaff = staffId ?? 0,
                            PodcastEpisodeId = EpisodeReport.PodcastEpisodeId,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };

                        var EpisodeReportReviewSession = await _podcastEpisodeReportReviewSessionGenericRepository.CreateAsync(newEpisodeReportReviewSession);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "PodcastEpisodeReportId", EpisodeReport.Id },
                        { "AccountId", EpisodeReport.AccountId },
                        { "PodcastEpisodeId", EpisodeReport.PodcastEpisodeId },
                        { "PodcastEpisodeReportTypeId", EpisodeReport.PodcastEpisodeReportTypeId },
                        { "CreatedAt", EpisodeReport.CreatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully create podcast episode report for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating podcast episode report for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create podcast Episode report failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Create podcast episode report failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<List<PodcastEpisodeReportTypeDTO>> GetPodcastEpisodeReportTypeAsync()
        {
            return await _podcastEpisodeReportTypeGenericRepository.FindAll()
                .Select(brt => new PodcastEpisodeReportTypeDTO()
                {
                    Id = brt.Id,
                    Name = brt.Name
                })
                .ToListAsync();
        }
        public async Task<List<PodcastEpisodeReportReviewSessionListItemResponseDTO>> GetEpisodeReportReviewSessionAsync(int? staffId, int roleId)
        {
            var query = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll(
                predicate: null
                ).ToListAsync();
            if (roleId == 3)
            {
                query = query.Where(pbrrs => pbrrs.AssignedStaff == staffId).ToList();
            }
            var podcastEpisodeReportReviewSession = (await Task.WhenAll(query.Select(async pbrrs =>
            {
                var Episode = await GetPodcastEpisode(pbrrs.PodcastEpisodeId);
                var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);
                return new PodcastEpisodeReportReviewSessionListItemResponseDTO()
                {
                    Id = pbrrs.Id,
                    PodcastEpisode = new PodcastEpisodeSnippetDTO()
                    {
                        Id = Episode != null && Episode["Id"] != null ? Episode.Value<Guid>("Id") : Guid.Empty,
                        Name = Episode != null && Episode["Name"] != null ? Episode.Value<string>("Name") : "",
                        MainImageFileKey = Episode != null && Episode["MainImageFileKey"] != null ? Episode.Value<string>("MainImageFileKey") : "",
                    },
                    AssignedStaff = new AssignedStaffSnippetDTO()
                    {
                        Id = staff.Id,
                        FullName = staff.FullName,
                        Email = staff.Email,
                        MainImageFileKey = staff.MainImageFileKey
                    },

                    IsResolved = pbrrs.IsResolved,
                    CreatedAt = pbrrs.CreatedAt,
                    UpdatedAt = pbrrs.UpdatedAt,
                };
            }))).ToList();
            return podcastEpisodeReportReviewSession;
        }
        public async Task<PodcastEpisodeReportReviewSessionDetailResponseDTO> GetEpisodeReportReviewSessionByIdAsync(Guid id)
        {
            var pbrrs = await _podcastEpisodeReportReviewSessionGenericRepository.FindByIdAsync(id);
            if (pbrrs == null)
                return null;

            var query = await _podcastEpisodeReportGenericRepository.FindAll(
                predicate: null,
                includeFunc: source => source
                    .Include(r => r.PodcastEpisodeReportType)
                )
                .Where(r => r.PodcastEpisodeId == pbrrs.PodcastEpisodeId && r.ResolvedAt == null)
                .ToListAsync();
            var podcastEpisodeReport = (await Task.WhenAll(query.Select(async pbr =>
            {
                var Episode = await GetPodcastEpisode(pbr.PodcastEpisodeId);
                return new PodcastEpisodeReportListItemResponseDTO()
                {
                    Id = pbr.Id,
                    Content = pbr.Content,
                    AccountId = pbr.AccountId,
                    PodcastEpisode = new PodcastEpisodeSnippetDTO()
                    {
                        Id = Episode != null && Episode["Id"] != null ? Episode.Value<Guid>("Id") : Guid.Empty,
                        Name = Episode != null && Episode["Name"] != null ? Episode.Value<string>("Name") : "",
                        MainImageFileKey = Episode != null && Episode["MainImageFileKey"] != null ? Episode.Value<string>("MainImageFileKey") : "",
                    },
                    PodcastEpisodeReportType = new PodcastEpisodeReportTypeDTO()
                    {
                        Id = pbr.PodcastEpisodeReportType.Id,
                        Name = pbr.PodcastEpisodeReportType.Name,
                    },
                    ResolvedAt = pbr.ResolvedAt,
                    CreatedAt = pbr.CreatedAt,
                };
            }))).ToList();

            var Episode = await GetPodcastEpisode(pbrrs.PodcastEpisodeId);
            var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);

            return new PodcastEpisodeReportReviewSessionDetailResponseDTO()
            {
                Id = pbrrs.Id,
                PodcastEpisode = new PodcastEpisodeSnippetDTO()
                {
                    Id = Episode != null && Episode["Id"] != null ? Episode.Value<Guid>("Id") : Guid.Empty,
                    Name = Episode != null && Episode["Name"] != null ? Episode.Value<string>("Name") : "",
                    MainImageFileKey = Episode != null && Episode["MainImageFileKey"] != null ? Episode.Value<string>("MainImageFileKey") : "",
                },
                AssignedStaff = new AssignedStaffSnippetDTO()
                {
                    Id = staff.Id,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    MainImageFileKey = staff.MainImageFileKey
                },
                IsResolved = pbrrs.IsResolved ?? false,
                CreatedAt = pbrrs.CreatedAt,
                UpdatedAt = pbrrs.UpdatedAt,
                EpisodeReportList = podcastEpisodeReport
            };
        }
        public async Task ResolveEpisodeReportReviewSessionAsync(ResolveEpisodeReportParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindByIdAsync(parameter.PodcastEpisodeReportReviewSessionId);
                    if (podcastEpisodeReportReviewSessions.AssignedStaff != parameter.AccountId)
                    {
                        throw new Exception("The logged in Account is not authorize to resolve this episode report review session");
                    }
                    podcastEpisodeReportReviewSessions.IsResolved = parameter.IsResolved;
                    var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastEpisodeId == podcastEpisodeReportReviewSessions.PodcastEpisodeId)
                        .ToListAsync();
                    foreach (var EpisodeReport in podcastEpisodeReportList)
                    {
                        EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                    }

                    if (parameter.IsTakenEffect && parameter.IsResolved)
                    {
                        var resolveRequestData = new JObject
                        {
                            { "PodcastEpisodeId", podcastEpisodeReportReviewSessions.PodcastEpisodeId }
                        };
                        var resolveReportMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.ContentManagementDomain,
                            requestData: resolveRequestData,
                            sagaInstanceId: null,
                            messageName: "episode-remove-flow");
                        await _messagingService.SendSagaMessageAsync(resolveReportMessage, null);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "IsResolved", podcastEpisodeReportReviewSessions.IsResolved },
                        { "IsTakenEffect", parameter.IsTakenEffect },
                        { "PodcastEpisodeReportReviewSessionId", podcastEpisodeReportReviewSessions.Id },
                        { "UpdatedAt", podcastEpisodeReportReviewSessions.UpdatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully resolve podcast episode report review session for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while resolving podcast episode report review session for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve podcast Episode report review session failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Resolve podcast episode report review session failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
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
        private async Task<JArray?> GetStaffList()
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeStaffList",
                            QueryType = "findall",
                            EntityType = "Account",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsVerify = true,
                                        RoleId = 3
                                    },
                                }),
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

            return result.Results?["activeStaffList"] is JArray staffListArray && staffListArray.Count > 0
                ? staffListArray as JArray
                : null;
        }
        private async Task<JObject?> GetActiveSystemConfigProfile()
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
                ? configArray.First as JObject
                : null;
        }
        private JToken? GetRandomItemFromJArray(JArray? array)
        {
            if (array == null || array.Count == 0)
                return null;

            var random = new Random();
            var randomIndex = random.Next(array.Count);
            return array[randomIndex];
        }
    }
}
