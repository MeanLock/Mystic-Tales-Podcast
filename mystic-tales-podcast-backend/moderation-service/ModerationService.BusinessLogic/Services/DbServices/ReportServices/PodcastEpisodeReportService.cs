using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Account;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateEpisodeReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelEpisodesReportNoEffectChannelDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelEpisodesReportNoEffectUnpublishChannelForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReportNoEffectDMCARemoveEpisodeForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReportNoEffectEpisodeDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowEpisodesReportNoEffectDMCARemoveShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowEpisodesReportNoEffectShowDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowEpisodesReportNoEffectUnpublishShowForce;
using ModerationService.BusinessLogic.DTOs.Podcast;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport.Details;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport.ListItems;
using ModerationService.BusinessLogic.DTOs.Snippet;
using ModerationService.BusinessLogic.DTOs.SystemConfiguration;
using ModerationService.BusinessLogic.Enums.Account;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Enums.Podcast;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.DataAccess.Repositories.interfaces;
using ModerationService.Infrastructure.Models.Kafka;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.Podcast;

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
            try
            {
                var query = await _podcastEpisodeReportGenericRepository.FindAll(
                    predicate: null,
                    includeFunc: source => source
                        .Include(r => r.PodcastEpisodeReportType)
                    ).ToListAsync();
                var podcastEpisodeReport = (await Task.WhenAll(query.Select(async pbr =>
                {
                    var episode = await GetPodcastEpisode(pbr.PodcastEpisodeId);
                    var account = await _accountCachingService.GetAccountStatusCacheById(pbr.AccountId);
                    return new PodcastEpisodeReportListItemResponseDTO()
                    {
                        Id = pbr.Id,
                        Content = pbr.Content,
                        Account = new AccountSnippetResponseDTO()
                        {
                            Id = account.Id,
                            FullName = account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastEpisode = new PodcastEpisodeSnippetResponseDTO()
                        {
                            Id = episode.Id,
                            Name = episode.Name,
                            MainImageFileKey = episode.MainImageFileKey,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all podcast episode reports");
                throw new HttpRequestException("Retrieve Episode Report failed. Error: " + ex.Message);
            }
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
                    if(systemConfig == null)
                    {
                        throw new Exception("System Configuration not found or inactive");
                    }

                    var validation = await ValidateEpisode(parameter.PodcastEpisodeId);
                    if (!validation.isValid)
                    {
                        throw new Exception(validation.errorMessage);
                    }

                    var existingReport = await _podcastEpisodeReportGenericRepository.FindAll()
                        .Where(br => br.PodcastEpisodeId == parameter.PodcastEpisodeId
                            && br.AccountId == parameter.AccountId
                            && br.PodcastEpisodeReportTypeId == parameter.PodcastEpisodeReportTypeId
                            && br.ResolvedAt == null)
                        .ToListAsync();

                    if (existingReport.Count() > 0)
                    {
                        throw new Exception("Duplicate report detected for the same episode by the same account with the same report type");
                    }

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
                    var existingEpisodeReportReviewSession = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                        .Where(brrs => brrs.PodcastEpisodeId == parameter.PodcastEpisodeId && brrs.IsResolved == null)
                        .ToListAsync();
                    if (existingEpisodeReportReviewSession.Count() == 0 || existingEpisodeReportReviewSession == null)
                    {
                        if (existingEpisodeReport.Count() >= systemConfig.ReviewSessionConfig.PodcastEpisodeUnResolvedReportStreak)
                        {
                            var staffList = await GetStaffList();
                            var randomStaff = await GetRandomStaffFromList(staffList);

                            var newEpisodeReportReviewSession = new PodcastEpisodeReportReviewSession()
                            {
                                AssignedStaff = randomStaff,
                                PodcastEpisodeId = EpisodeReport.PodcastEpisodeId,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };

                            var EpisodeReportReviewSession = await _podcastEpisodeReportReviewSessionGenericRepository.CreateAsync(newEpisodeReportReviewSession);
                        }
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
        public async Task<List<PodcastEpisodeReportTypeDTO>> GetAllPodcastEpisodeReportTypeAsync()
        {
            try
            {
                var query = _podcastEpisodeReportTypeGenericRepository.FindAll();
                return await query
                .Select(brt => new PodcastEpisodeReportTypeDTO()
                {
                    Id = brt.Id,
                    Name = brt.Name
                })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving podcast episode report types");
                throw new HttpRequestException("Retrieve Episode Report Types failed. Error: " + ex.Message);
            }
        }
        public async Task<List<PodcastEpisodeReportTypeDTO>> GetPodcastEpisodeReportTypeAsync(AccountStatusCache? account, Guid podcastEpisodeId)
        {
            try
            {
                var query = _podcastEpisodeReportTypeGenericRepository.FindAll();
                if (account != null)
                {
                    var reportList = await _podcastEpisodeReportGenericRepository.FindAll()
                    .Where(r => r.AccountId == account.Id && r.ResolvedAt == null && r.PodcastEpisodeId == podcastEpisodeId)
                    .Select(r => r.PodcastEpisodeReportTypeId)
                    .Distinct()
                    .ToListAsync();
                    query = query.Where(brt => !reportList.Contains(brt.Id));
                }
                return await query
                .Select(brt => new PodcastEpisodeReportTypeDTO()
                {
                    Id = brt.Id,
                    Name = brt.Name
                })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving podcast episode report types");
                throw new HttpRequestException("Retrieve Episode Report Types failed. Error: " + ex.Message);
            }
        }
        public async Task<List<PodcastEpisodeReportReviewSessionListItemResponseDTO>> GetEpisodeReportReviewSessionAsync(int? staffId, int roleId)
        {
            try
            {
                var query = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll(
                    predicate: null
                    ).ToListAsync();
                if (roleId == (int)RoleEnum.Staff)
                {
                    query = query.Where(pbrrs => pbrrs.AssignedStaff == staffId).ToList();
                }
                var podcastEpisodeReportReviewSession = (await Task.WhenAll(query.OrderByDescending(pbrrs => pbrrs.CreatedAt).Select(async pbrrs =>
                {
                    var episode = await GetPodcastEpisode(pbrrs.PodcastEpisodeId);
                    var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);
                    return new PodcastEpisodeReportReviewSessionListItemResponseDTO()
                    {
                        Id = pbrrs.Id,
                        PodcastEpisode = new PodcastEpisodeSnippetResponseDTO()
                        {
                            Id = episode.Id,
                            Name = episode.Name,
                            MainImageFileKey = episode.MainImageFileKey,
                        },
                        AssignedStaff = new AssignedStaffSnippetResponseDTO()
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving podcast episode report review sessions");
                throw new HttpRequestException("Retrieve Episode Report Review Sessions failed. Error: " + ex.Message);
            }
        }
        public async Task<PodcastEpisodeReportReviewSessionDetailResponseDTO> GetEpisodeReportReviewSessionByIdAsync(Guid id)
        {
            try
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
                    var episode = await GetPodcastEpisode(pbr.PodcastEpisodeId);
                    var account = await _accountCachingService.GetAccountStatusCacheById(pbr.AccountId);

                    return new PodcastEpisodeReportListItemResponseDTO()
                    {
                        Id = pbr.Id,
                        Content = pbr.Content,
                        Account = new AccountSnippetResponseDTO()
                        {
                            Id = account.Id,
                            FullName = account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastEpisode = new PodcastEpisodeSnippetResponseDTO()
                        {
                            Id = episode.Id,
                            Name = episode.Name,
                            MainImageFileKey = episode.MainImageFileKey,
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

                var episode = await GetPodcastEpisode(pbrrs.PodcastEpisodeId);
                var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);

                return new PodcastEpisodeReportReviewSessionDetailResponseDTO()
                {
                    Id = pbrrs.Id,
                    PodcastEpisode = new PodcastEpisodeSnippetResponseDTO()
                    {
                        Id = episode.Id,
                        Name = episode.Name,
                        MainImageFileKey = episode.MainImageFileKey,
                    },
                    AssignedStaff = new AssignedStaffSnippetResponseDTO()
                    {
                        Id = staff.Id,
                        FullName = staff.FullName,
                        Email = staff.Email,
                        MainImageFileKey = staff.MainImageFileKey
                    },
                    IsResolved = pbrrs.IsResolved,
                    CreatedAt = pbrrs.CreatedAt,
                    UpdatedAt = pbrrs.UpdatedAt,
                    EpisodeReportList = pbrrs.IsResolved != null ? null : podcastEpisodeReport
                };


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving podcast episode report review session by id");
                throw new HttpRequestException("Retrieve Episode Report Review Session by Id failed. Error: " + ex.Message);
            }
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
                    await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(podcastEpisodeReportReviewSessions.Id, podcastEpisodeReportReviewSessions);
                    var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastEpisodeId == podcastEpisodeReportReviewSessions.PodcastEpisodeId
                        && pbr.ResolvedAt == null)
                        .ToListAsync();
                    foreach (var EpisodeReport in podcastEpisodeReportList)
                    {
                        EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                    }

                    if (parameter.IsTakenEffect && parameter.IsResolved)
                    {
                        var validation = await ValidateEpisode(podcastEpisodeReportReviewSessions.PodcastEpisodeId);
                        if (!validation.isValid)
                        {
                            _logger.LogInformation(validation.errorMessage);
                        }
                        else
                        {
                            var resolveRequestData = new JObject
                            {
                                { "PodcastEpisodeId", podcastEpisodeReportReviewSessions.PodcastEpisodeId }
                            };
                            var resolveReportMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ContentManagementDomain,
                                requestData: resolveRequestData,
                                sagaInstanceId: null,
                                messageName: "dmca-remove-episode-flow");
                            await _messagingService.SendSagaMessageAsync(resolveReportMessage, null);
                        }
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
        public async Task ResolveEpisodeReportNoEffectDMCARemoveEpisodeForceAsync(ResolveEpisodeReportNoEffectDMCARemoveEpisodeForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                    .Where(errs => errs.PodcastEpisodeId == parameter.PodcastEpisodeId && errs.IsResolved == null).ToListAsync();
                    foreach (var session in podcastEpisodeReportReviewSessions)
                    {
                        session.IsResolved = true;
                        session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                        var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                        && pbr.ResolvedAt == null)
                        .ToListAsync();
                        foreach (var EpisodeReport in podcastEpisodeReportList)
                        {
                            EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Episode Report No Effect DMCA Remove Episode Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Episode Report No Effect DMCA Remove Episode Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Episode Report No Effect DMCA Remove Episode Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Episode Report No Effect DMCA Remove Episode Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveShowEpisodesReportNoEffectDMCARemoveShowForceAsync(ResolveShowEpisodesReportNoEffectDMCARemoveShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var episodeList = await GetPodcastEpisodeByShowId(parameter.PodcastShowId);
                    if(episodeList != null)
                    {
                        foreach (var episodeId in episodeList.Select(e => e.Id).ToList())
                        {
                            var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                            .Where(errs => errs.PodcastEpisodeId == episodeId && errs.IsResolved == null).ToListAsync();
                            foreach (var session in podcastEpisodeReportReviewSessions)
                            {
                                session.IsResolved = true;
                                session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                                .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                                && pbr.ResolvedAt == null)
                                .ToListAsync();
                                foreach (var EpisodeReport in podcastEpisodeReportList)
                                {
                                    EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                                }
                            }
                        }
                    } else
                    {
                        _logger.LogInformation("Unable to query episode for PodcastShowId: {PodcastShowId}", parameter.PodcastShowId);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Show Episodes Report No Effect DMCA Remove Show Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Show Episodes Report No Effect DMCA Remove Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Show Episodes Report No Effect DMCA Remove Show Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Show Episodes Report No Effect DMCA Remove Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveShowEpisodesReportNoEffectUnpublishShowForceAsync(ResolveShowEpisodesReportNoEffectUnpublishShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var episodeIdList = parameter.DmcaDismissedEpisodeIds;
                    if (episodeIdList != null && episodeIdList.Count > 0)
                    {
                        foreach (var episodeId in episodeIdList)
                        {
                            var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                            .Where(errs => errs.PodcastEpisodeId == episodeId && errs.IsResolved == null).ToListAsync();

                            foreach (var session in podcastEpisodeReportReviewSessions)
                            {
                                session.IsResolved = true;
                                session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                                .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                                && pbr.ResolvedAt == null)
                                .ToListAsync();
                                foreach (var EpisodeReport in podcastEpisodeReportList)
                                {
                                    EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Show Episodes Report No Effect Unpublish Show Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Show Episodes Report No Effect Unpublish Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Show Episodes Report No Effect Unpublish Show Forc failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Show Episodes Report No Effect Unpublish Show Forc failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveChannelEpisodesReportNoEffectUnpublishChannelForceAsync(ResolveChannelEpisodesReportNoEffectUnpublishChannelForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var episodeIdList = parameter.DmcaDismissedEpisodeIds;
                    if (episodeIdList != null && episodeIdList.Count > 0)
                    {
                        foreach (var episodeId in episodeIdList)
                        {
                            var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                            .Where(errs => errs.PodcastEpisodeId == episodeId && errs.IsResolved == null).ToListAsync();

                            foreach (var session in podcastEpisodeReportReviewSessions)
                            {
                                session.IsResolved = true;
                                session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                                .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                                && pbr.ResolvedAt == null)
                                .ToListAsync();
                                foreach (var EpisodeReport in podcastEpisodeReportList)
                                {
                                    EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Channel Episodes Report No Effect Unpublish Channel Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Channel Episodes Report No Effect Unpublish Channel Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject {
                        { "ErrorMessage", "Resolve Channel Episodes Report No Effect Unpublish Channel Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Channel Episodes Report No Effect Unpublish Channel Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveEpisodeReportNoEffectEpisodeDeletionForceAsync(ResolveEpisodeReportNoEffectEpisodeDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                    .Where(errs => errs.PodcastEpisodeId == parameter.PodcastEpisodeId && errs.IsResolved == null).ToListAsync();
                    foreach (var session in podcastEpisodeReportReviewSessions)
                    {
                        session.IsResolved = true;
                        session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                        var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                        && pbr.ResolvedAt == null)
                        .ToListAsync();
                        foreach (var EpisodeReport in podcastEpisodeReportList)
                        {
                            EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Episode Report No Effect Episode Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Episode Report No Effect Episode Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject {
                        { "ErrorMessage", "Resolve Episode Report No Effect Episode Deletion Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Episode Report No Effect Episode Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveShowEpisodesReportNoEffectShowDeletionForceAsync(ResolveShowEpisodesReportNoEffectShowDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var episodeList = await GetPodcastEpisodeByShowId(parameter.PodcastShowId);
                    if(episodeList != null)
                    {
                        foreach (var episodeId in episodeList.Select(e => e.Id).ToList())
                        {
                            var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                            .Where(errs => errs.PodcastEpisodeId == episodeId && errs.IsResolved == null).ToListAsync();
                            foreach (var session in podcastEpisodeReportReviewSessions)
                            {
                                session.IsResolved = true;
                                session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                                .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                                && pbr.ResolvedAt == null)
                                .ToListAsync();
                                foreach (var EpisodeReport in podcastEpisodeReportList)
                                {
                                    EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Unable to query episode for PodcastShowId: {PodcastShowId}", parameter.PodcastShowId);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Show Episodes Report No Effect Show Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Show Episodes Report No Effect Show Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject {
                        { "ErrorMessage", "Resolve Show Episodes Report No Effect Show Deletion Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Show Episodes Report No Effect Show Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveChannelEpisodesReportNoEffectChannelDeletionForceAsync(ResolveChannelEpisodesReportNoEffectChannelDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var showIdList = parameter.DmcaDismissedShowIds;
                    if (showIdList != null && showIdList.Count > 0)
                    {
                        foreach (var showId in showIdList)
                        {
                            var episodeList = await GetPodcastEpisodeByShowId(showId);
                            if(episodeList != null)
                            {
                                foreach (var episodeId in episodeList.Select(e => e.Id).ToList())
                                {
                                    var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                                    .Where(errs => errs.PodcastEpisodeId == episodeId && errs.IsResolved == null).ToListAsync();
                                    foreach (var session in podcastEpisodeReportReviewSessions)
                                    {
                                        session.IsResolved = true;
                                        session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                        await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);
                                        var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                                        .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                                        && pbr.ResolvedAt == null)
                                        .ToListAsync();
                                        foreach (var EpisodeReport in podcastEpisodeReportList)
                                        {
                                            EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                            await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                                        }
                                    }
                                }
                            } else
                            {
                                _logger.LogInformation("Unable to query episode for PodcastShowId: {PodcastShowId}", showId);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Channel Episodes Report No Effect Channel Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Channel Episodes Report No Effect Channel Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject {
                        { "ErrorMessage", "Resolve Channel Episodes Report No Effect Channel Deletion Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Channel Episodes Report No Effect Channel Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForceAsync(ResolvePodcasterEpisodesReportNoEffectTerminatePodcasterForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var showList = await GetPodcastShowByPodcasterId(parameter.PodcasterId);
                    
                    if (showList != null)
                    {
                        foreach(var show in showList)
                        {
                            if(show.PodcastEpisodes == null || show.PodcastEpisodes.Count == 0)
                            {
                                continue;
                            }
                            foreach (var episodeId in show.PodcastEpisodes.Select(s => s.Id))
                            {
                                var podcastEpisodeReportReviewSessions = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                                .Where(errs => errs.PodcastEpisodeId == episodeId && errs.IsResolved == null).ToListAsync();

                                foreach (var session in podcastEpisodeReportReviewSessions)
                                {
                                    session.IsResolved = true;
                                    session.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastEpisodeReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                    var podcastEpisodeReportList = await _podcastEpisodeReportGenericRepository.FindAll()
                                    .Where(pbr => pbr.PodcastEpisodeId == session.PodcastEpisodeId
                                    && pbr.ResolvedAt == null)
                                    .ToListAsync();
                                    foreach (var EpisodeReport in podcastEpisodeReportList)
                                    {
                                        EpisodeReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                        await _podcastEpisodeReportGenericRepository.UpdateAsync(EpisodeReport.Id, EpisodeReport);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Unable to query show for PodcasterId: {PodcasterId}", parameter.PodcasterId);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ReportManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Resolve Podcaster Episodes Report No Effect Terminate Podcaster Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Podcaster Episodes Report No Effect Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject {
                        { "ErrorMessage", "Resolve Podcaster Episodes Report No Effect Terminate Podcaster Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Podcaster Episodes Report No Effect Terminate Podcaster Force failed for SagaId: {SagaId}", command.SagaInstanceId);
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
            } catch (Exception ex)
            {
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching podcast episode from Podcast Service");
                throw new HttpRequestException("Retreive Podcast Episode failed. Error: " + ex.Message);
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
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching podcast episodes by show Id from Podcast Service");
                throw new HttpRequestException("Retreive Podcast Episodes by Show Id failed. Error: " + ex.Message);
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
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching podcast channel from Podcast Service");
                throw new HttpRequestException("Retreive Podcast Channel failed. Error: " + ex.Message);
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
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching podcast show from Podcast Service");
                throw new HttpRequestException("Retreive Podcast Show failed. Error: " + ex.Message);
            }
        }
        private async Task<List<AccountDTO>?> GetStaffList()
        {
            try
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
                                        RoleId = (int)RoleEnum.Staff
                                    },
                                }),
                        }
                    }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

                return result.Results?["activeStaffList"] is JArray staffListArray && staffListArray.Count >= 0
                    ? staffListArray.ToObject<List<AccountDTO>>()
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching staff list from User Service");
                throw new HttpRequestException("Retreive Staff list failed. Error: " + ex.Message);
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
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching active system configuration profile from System Configuration Service");
                throw new HttpRequestException("Retreive Active System Configuration Profile failed. Error: " + ex.Message);
            }
        }
        private async Task<int> GetRandomStaffFromList(List<AccountDTO>? array)
        {
            //if (array == null || array.Count == 0)
            //    return null;

            //var random = new Random();
            //var randomIndex = random.Next(array.Count);
            //return array[randomIndex];

            var assignedStaffIds = await _podcastEpisodeReportReviewSessionGenericRepository.FindAll()
                .Where(per => per.IsResolved == null)
                .Select(pbrrs => pbrrs.AssignedStaff)
                .ToListAsync();

            List<AccountDTO> availableStaff = array;
            Dictionary<int, int> staffAssignmentCount = new Dictionary<int, int>();
            foreach (var staff in availableStaff)
            {
                int count = assignedStaffIds.Count(id => id == staff.Id);
                staffAssignmentCount[staff.Id] = count;
            }

            int minAssignmentCount = staffAssignmentCount.Values.Min();
            List<int> leastAssignedStaffIds = staffAssignmentCount
                .Where(kvp => kvp.Value == minAssignmentCount)
                .Select(kvp => kvp.Key)
                .ToList();
            Random rand = new Random();
            int randomIndex = rand.Next(leastAssignedStaffIds.Count);
            return leastAssignedStaffIds[randomIndex];
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n exception: " + ex.Message + "\n");
                _logger.LogError(ex, "Error occurred while fetching podcast shows by podcaster Id from Podcast Service");
                throw new HttpRequestException("Retreive Podcast Shows by Podcaster Id failed. Error: " + ex.Message);
            }
        }
    }
}
