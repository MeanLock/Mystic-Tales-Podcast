using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateShowReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastShowReport;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport.Details;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport.ListItems;
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
    public class PodcastShowReportService
    {
        private readonly IGenericRepository<PodcastShowReport> _podcastShowReportGenericRepository;
        private readonly IGenericRepository<PodcastShowReportReviewSession> _podcastShowReportReviewSessionGenericRepository;
        private readonly IGenericRepository<PodcastShowReportType> _podcastShowReportTypeGenericRepository;

        private readonly AccountCachingService _accountCachingService;

        private readonly ILogger<PodcastShowReportService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public PodcastShowReportService(
            IGenericRepository<PodcastShowReport> podcastShowReportGenericRepository, 
            IGenericRepository<PodcastShowReportReviewSession> podcastShowReportReviewSessionGenericRepository, 
            IGenericRepository<PodcastShowReportType> podcastShowReportTypeGenericRepository, 
            AccountCachingService accountCachingService, 
            ILogger<PodcastShowReportService> logger, 
            KafkaProducerService kafkaProducerService, 
            IMessagingService messagingService, 
            HttpServiceQueryClient httpServiceQueryClient, 
            AppDbContext appDbContext, 
            DateHelper dateHelper)
        {
            _podcastShowReportGenericRepository = podcastShowReportGenericRepository;
            _podcastShowReportReviewSessionGenericRepository = podcastShowReportReviewSessionGenericRepository;
            _podcastShowReportTypeGenericRepository = podcastShowReportTypeGenericRepository;
            _accountCachingService = accountCachingService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<List<PodcastShowReportListItemResponseDTO>> GetAllPodcastShowReportAsync()
        {
            var query = await _podcastShowReportGenericRepository.FindAll(
                predicate: null,
                includeFunc: source => source
                    .Include(r => r.PodcastShowReportType)
                ).ToListAsync();
            var podcastShowReport = (await Task.WhenAll(query.Select(async pbr =>
            {
                var show = await GetPodcastShow(pbr.PodcastShowId);
                return new PodcastShowReportListItemResponseDTO()
                {
                    Id = pbr.Id,
                    Content = pbr.Content,
                    AccountId = pbr.AccountId,
                    PodcastShow = new PodcastShowSnippetDTO()
                    {
                        Id = show != null && show["Id"] != null ? show.Value<Guid>("Id") : Guid.Empty,
                        Name = show != null && show["Name"] != null ? show.Value<string>("Name") : "",
                        MainImageFileKey = show != null && show["MainImageFileKey"] != null ? show.Value<string>("MainImageFileKey") : "",
                    },
                    PodcastShowReportType = new PodcastShowReportTypeDTO()
                    {
                        Id = pbr.PodcastShowReportType.Id,
                        Name = pbr.PodcastShowReportType.Name,
                    },
                    ResolvedAt = pbr.ResolvedAt,
                    CreatedAt = pbr.CreatedAt,
                };
            }))).ToList();
            return podcastShowReport;
        }
        public async Task CreatePodcastShowReportAsync(CreateShowReportParameterDTO parameter, SagaCommandMessage command)
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

                    var newShowReport = new PodcastShowReport()
                    {
                        AccountId = parameter.AccountId,
                        Content = parameter.Content,
                        PodcastShowId = parameter.PodcastShowId,
                        PodcastShowReportTypeId = parameter.PodcastShowReportTypeId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };

                    var ShowReport = await _podcastShowReportGenericRepository.CreateAsync(newShowReport);

                    var existingShowReport = await _podcastShowReportGenericRepository.FindAll()
                        .Where(br => br.PodcastShowId == parameter.PodcastShowId && br.ResolvedAt == null)
                        .ToListAsync();
                    if (existingShowReport.Count() >= systemConfig["ReviewSessionConfig"].Value<int>("PodcastShowUnResolvedReportStreak"))
                    {
                        var staffList = await GetStaffList();
                        var randomStaff = GetRandomItemFromJArray(staffList);
                        var staffId = randomStaff["Id"]?.Value<int>();

                        var newShowReportReviewSession = new PodcastShowReportReviewSession()
                        {
                            AssignedStaff = staffId ?? 0,
                            PodcastShowId = ShowReport.PodcastShowId,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };

                        var ShowReportReviewSession = await _podcastShowReportReviewSessionGenericRepository.CreateAsync(newShowReportReviewSession);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "PodcastShowReportId", ShowReport.Id },
                        { "AccountId", ShowReport.AccountId },
                        { "PodcastShowId", ShowReport.PodcastShowId },
                        { "PodcastShowReportTypeId", ShowReport.PodcastShowReportTypeId },
                        { "CreatedAt", ShowReport.CreatedAt }
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
                    _logger.LogInformation("Successfully create podcast show report for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while create podcast show report for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create podcast show report failed, error: " + ex.Message }
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
                    _logger.LogInformation("Create podcast show report failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<List<PodcastShowReportTypeDTO>> GetPodcastShowReportTypeAsync()
        {
            return await _podcastShowReportTypeGenericRepository.FindAll()
                .Select(brt => new PodcastShowReportTypeDTO()
                {
                    Id = brt.Id,
                    Name = brt.Name
                })
                .ToListAsync();
        }
        public async Task<List<PodcastShowReportReviewSessionListItemResponseDTO>> GetShowReportReviewSessionAsync(int? staffId, int roleId)
        {
            var query = await _podcastShowReportReviewSessionGenericRepository.FindAll(
                predicate: null
                ).ToListAsync();
            if (roleId == 3)
            {
                query = query.Where(pbrrs => pbrrs.AssignedStaff == staffId).ToList();
            }
            var podcastShowReportReviewSession = (await Task.WhenAll(query.Select(async pbrrs =>
            {
                var show = await GetPodcastShow(pbrrs.PodcastShowId);
                var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);
                return new PodcastShowReportReviewSessionListItemResponseDTO()
                {
                    Id = pbrrs.Id,
                    PodcastShow = new PodcastShowSnippetDTO()
                    {
                        Id = show != null && show["Id"] != null ? show.Value<Guid>("Id") : Guid.Empty,
                        Name = show != null && show["Name"] != null ? show.Value<string>("Name") : "",
                        MainImageFileKey = show != null && show["MainImageFileKey"] != null ? show.Value<string>("MainImageFileKey") : "",
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
            return podcastShowReportReviewSession;
        }
        public async Task<PodcastShowReportReviewSessionDetailResponseDTO> GetShowReportReviewSessionByIdAsync(Guid id)
        {
            var pbrrs = await _podcastShowReportReviewSessionGenericRepository.FindByIdAsync(id);
            if (pbrrs == null)
                return null;

            var query = await _podcastShowReportGenericRepository.FindAll(
                predicate: null,
                includeFunc: source => source
                    .Include(r => r.PodcastShowReportType)
                )
                .Where(r => r.PodcastShowId == pbrrs.PodcastShowId && r.ResolvedAt == null)
                .ToListAsync();
            var podcastShowReport = (await Task.WhenAll(query.Select(async pbr =>
            {
                var show = await GetPodcastShow(pbr.PodcastShowId);
                return new PodcastShowReportListItemResponseDTO()
                {
                    Id = pbr.Id,
                    Content = pbr.Content,
                    AccountId = pbr.AccountId,
                    PodcastShow = new PodcastShowSnippetDTO()
                    {
                        Id = show != null && show["Id"] != null ? show.Value<Guid>("Id") : Guid.Empty,
                        Name = show != null && show["Name"] != null ? show.Value<string>("Name") : "",
                        MainImageFileKey = show != null && show["MainImageFileKey"] != null ? show.Value<string>("MainImageFileKey") : "",
                    },
                    PodcastShowReportType = new PodcastShowReportTypeDTO()
                    {
                        Id = pbr.PodcastShowReportType.Id,
                        Name = pbr.PodcastShowReportType.Name,
                    },
                    ResolvedAt = pbr.ResolvedAt,
                    CreatedAt = pbr.CreatedAt,
                };
            }))).ToList();

            var show = await GetPodcastShow(pbrrs.PodcastShowId);
            var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);

            return new PodcastShowReportReviewSessionDetailResponseDTO()
            {
                Id = pbrrs.Id,
                PodcastShow = new PodcastShowSnippetDTO()
                {
                    Id = show != null && show["Id"] != null ? show.Value<Guid>("Id") : Guid.Empty,
                    Name = show != null && show["Name"] != null ? show.Value<string>("Name") : "",
                    MainImageFileKey = show != null && show["MainImageFileKey"] != null ? show.Value<string>("MainImageFileKey") : "",
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
                ShowReportList = podcastShowReport
            };
        }
        public async Task ResolveShowReportReviewSessionAsync(ResolveShowReportParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastShowReportReviewSessions = await _podcastShowReportReviewSessionGenericRepository.FindByIdAsync(parameter.PodcastShowReportReviewSessionId);
                    if (podcastShowReportReviewSessions.AssignedStaff != parameter.AccountId)
                    {
                        throw new Exception("The logged in Account is not authorize to resolve this show report review session");
                    }
                    podcastShowReportReviewSessions.IsResolved = parameter.IsResolved;
                    var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastShowId == podcastShowReportReviewSessions.PodcastShowId)
                        .ToListAsync();
                    foreach (var ShowReport in podcastShowReportList)
                    {
                        ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
                    }

                    if (parameter.IsTakenEffect && parameter.IsResolved)
                    {
                        var resolveRequestData = new JObject
                        {
                            { "PodcastShowId", podcastShowReportReviewSessions.PodcastShowId }
                        };
                        var resolveReportMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.ContentManagementDomain,
                            requestData: resolveRequestData,
                            sagaInstanceId: null,
                            messageName: "show-remove-flow");
                        await _messagingService.SendSagaMessageAsync(resolveReportMessage, null);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "IsResolved", podcastShowReportReviewSessions.IsResolved },
                        { "IsTakenEffect", parameter.IsTakenEffect },
                        { "PodcastShowReportReviewSessionId", podcastShowReportReviewSessions.Id },
                        { "UpdatedAt", podcastShowReportReviewSessions.UpdatedAt }
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
                    _logger.LogInformation("Successfully resolve podcast show report review session for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while resolve podcast show report review session for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve podcast show report review session failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve podcast show report review session failed for SagaId: {SagaId}", command.SagaInstanceId);
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
                                Fields = new[] { "Id", "FullName", "Email", "MainImageFileKey" }
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
