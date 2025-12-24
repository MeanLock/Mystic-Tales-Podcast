using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Account;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateShowReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelShowsReportNoEffectChannelDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelShowsReportNoEffectUnpublishChannelForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcasterShowsReportNoEffectTerminatePodcasterForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastShowReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowReportNoEffectDMCARemoveShowForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowReportNoEffectShowDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveShowReportNoEffectUnpublishShowForce;
using ModerationService.BusinessLogic.DTOs.Podcast;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport.Details;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport.ListItems;
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
            try
            {
                var query = await _podcastShowReportGenericRepository.FindAll(
                    predicate: null,
                    includeFunc: source => source
                        .Include(r => r.PodcastShowReportType)
                    ).ToListAsync();
                var podcastShowReport = (await Task.WhenAll(query.Select(async pbr =>
                {
                    var show = await GetPodcastShow(pbr.PodcastShowId);
                    if(show == null)
                    {
                        _logger.LogWarning("Unable to query Podcast show with ID {PodcastShowId}", pbr.PodcastShowId);
                        return null;
                    }
                    var account = await _accountCachingService.GetAccountStatusCacheById(pbr.AccountId);
                    return new PodcastShowReportListItemResponseDTO()
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
                        PodcastShow = new PodcastShowSnippetResponseDTO()
                        {
                            Id = show.Id,
                            Name = show.Name,
                            MainImageFileKey = show.MainImageFileKey,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all podcast show reports");
                throw new HttpRequestException("Retrieve Show report failed. Error: "+ ex.Message);
            }
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
                    if(systemConfig == null)
                    {
                        throw new Exception("Unable to query system configuration");
                    }

                    var validation = await ValidateShow(parameter.PodcastShowId);
                    if (!validation.isValid)
                    {
                        throw new Exception(validation.errorMessage);
                    }

                    var existingReport = await _podcastShowReportGenericRepository.FindAll()
                        .Where(br => br.AccountId == parameter.AccountId && br.PodcastShowId == parameter.PodcastShowId && br.PodcastShowReportTypeId == parameter.PodcastShowReportTypeId && br.ResolvedAt == null)
                        .ToListAsync();

                    if (existingReport.Count() > 0)
                    {
                        throw new Exception("You have already reported this podcast show for the same reason");
                    }

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
                    var existingShowReportReviewSession = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                        .Where(bsrrs => bsrrs.PodcastShowId == parameter.PodcastShowId && bsrrs.IsResolved == null)
                        .ToListAsync();
                    if(existingShowReportReviewSession.Count() == 0 || existingShowReportReviewSession == null)
                    {
                        if (existingShowReport.Count() >= systemConfig.ReviewSessionConfig.PodcastShowUnResolvedReportStreak)
                        {
                            var staffList = await GetStaffList();
                            if(staffList == null)
                            {
                                throw new HttpRequestException("Unable to query staff");
                            }
                            var randomStaff = await GetRandomStaffFromList(staffList);

                            var newShowReportReviewSession = new PodcastShowReportReviewSession()
                            {
                                AssignedStaff = randomStaff,
                                PodcastShowId = ShowReport.PodcastShowId,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };

                            var ShowReportReviewSession = await _podcastShowReportReviewSessionGenericRepository.CreateAsync(newShowReportReviewSession);
                        }
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
        public async Task<List<PodcastShowReportTypeDTO>> GetAllPodcastShowReportTypeAsync()
        {
            try
            {
                var query = _podcastShowReportTypeGenericRepository.FindAll();
                return await query
                    .Select(brt => new PodcastShowReportTypeDTO()
                    {
                        Id = brt.Id,
                        Name = brt.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast show report types");
                throw new HttpRequestException("Retrieve Show report types failed. Error: " + ex.Message);
            }
        }
        public async Task<List<PodcastShowReportTypeDTO>> GetPodcastShowReportTypeAsync(AccountStatusCache? account, Guid podcastShowId)
        {
            try
            {
                var query = _podcastShowReportTypeGenericRepository.FindAll();
                if (account != null)
                {
                    var reportList = await _podcastShowReportGenericRepository.FindAll()
                        .Where(r => r.AccountId == account.Id && r.ResolvedAt == null && r.PodcastShowId == podcastShowId)
                        .Select(r => r.PodcastShowReportTypeId)
                        .Distinct()
                        .ToListAsync();
                    query = query.Where(brt => !reportList.Contains(brt.Id));
                }
                return await query
                    .Select(brt => new PodcastShowReportTypeDTO()
                    {
                        Id = brt.Id,
                        Name = brt.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast show report types");
                throw new HttpRequestException("Retrieve Show report types failed. Error: " + ex.Message);
            }
        }
        public async Task<List<PodcastShowReportReviewSessionListItemResponseDTO>> GetShowReportReviewSessionAsync(int? staffId, int roleId)
        {
            try
            {
                var query = await _podcastShowReportReviewSessionGenericRepository.FindAll(
                    predicate: null
                    ).ToListAsync();
                if (roleId == (int)RoleEnum.Staff)
                {
                    query = query.Where(pbrrs => pbrrs.AssignedStaff == staffId).ToList();
                }
                var podcastShowReportReviewSession = (await Task.WhenAll(query.OrderByDescending(pbrrs => pbrrs.CreatedAt).Select(async pbrrs =>
                {
                    var show = await GetPodcastShow(pbrrs.PodcastShowId);
                    if(show == null)
                    {
                        _logger.LogWarning("Unable to query Podcast show with ID {PodcastShowId}", pbrrs.PodcastShowId);
                        return null;
                    }
                    var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);
                    return new PodcastShowReportReviewSessionListItemResponseDTO()
                    {
                        Id = pbrrs.Id,
                        PodcastShow = new PodcastShowSnippetResponseDTO()
                        {
                            Id = show.Id,
                            Name = show.Name,
                            MainImageFileKey = show.MainImageFileKey,
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
                return podcastShowReportReviewSession;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast show report review sessions");
                throw new HttpRequestException("Retrieve Show report review sessions failed. Error: " + ex.Message);
            }
        }
        public async Task<PodcastShowReportReviewSessionDetailResponseDTO> GetShowReportReviewSessionByIdAsync(Guid id)
        {
            try
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
                    if(show == null)
                    {
                        _logger.LogWarning("Unable to query Podcast show with ID {PodcastShowId}", pbr.PodcastShowId);
                        return null;
                    }
                    var account = await _accountCachingService.GetAccountStatusCacheById(pbr.AccountId);
                    return new PodcastShowReportListItemResponseDTO()
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
                        PodcastShow = new PodcastShowSnippetResponseDTO()
                        {
                            Id = show.Id,
                            Name = show.Name,
                            MainImageFileKey = show.MainImageFileKey,
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
                if(show == null)
                {
                    _logger.LogWarning("Unable to query Podcast show with ID {PodcastShowId}", pbrrs.PodcastShowId);
                    return null;
                }
                var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);

                return new PodcastShowReportReviewSessionDetailResponseDTO()
                {
                    Id = pbrrs.Id,
                    PodcastShow = new PodcastShowSnippetResponseDTO()
                    {
                        Id = show.Id,
                        Name = show.Name,
                        MainImageFileKey = show.MainImageFileKey,
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
                    ShowReportList = pbrrs.IsResolved != null ? null : podcastShowReport
                };


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast show report review session by id");
                throw new HttpRequestException("Retrieve Show report review session by id failed. Error: " + ex.Message);
            }
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
                    await _podcastShowReportReviewSessionGenericRepository.UpdateAsync(podcastShowReportReviewSessions.Id, podcastShowReportReviewSessions);
                    var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastShowId == podcastShowReportReviewSessions.PodcastShowId
                        && pbr.ResolvedAt == null)
                        .ToListAsync();
                    foreach (var ShowReport in podcastShowReportList)
                    {
                        ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
                    }

                    if (parameter.IsTakenEffect && parameter.IsResolved)
                    {
                        var validation = await ValidateShow(podcastShowReportReviewSessions.PodcastShowId);
                        if (!validation.isValid)
                        {
                            _logger.LogInformation(validation.errorMessage);
                        }
                        else
                        {
                            var resolveRequestData = new JObject
                            {
                                { "PodcastShowId", podcastShowReportReviewSessions.PodcastShowId }
                            };
                            var resolveReportMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.ContentManagementDomain,
                                requestData: resolveRequestData,
                                sagaInstanceId: null,
                                messageName: "dmca-remove-show-flow");
                            await _messagingService.SendSagaMessageAsync(resolveReportMessage, null);
                        }  
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
        public async Task ResolveShowReportNoEffectDMCARemoveShowForceAsync(ResolveShowReportNoEffectDMCARemoveShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastShowReportReviewSessions = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                        .Where(psrrs => psrrs.PodcastShowId == parameter.PodcastShowId)
                        .ToListAsync();

                    foreach(var session in podcastShowReportReviewSessions)
                    {
                        session.IsResolved = true;
                        await _podcastShowReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                        var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastShowId == session.PodcastShowId
                        && pbr.ResolvedAt == null)
                        .ToListAsync();
                        foreach (var ShowReport in podcastShowReportList)
                        {
                            ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
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
                    _logger.LogInformation("Successfully Resolve Show Report No Effect DMCA Remove Show Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Show Report No Effect DMCA Remove Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Show Report No Effect DMCA Remove Show Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Show Report No Effect DMCA Remove Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveShowReportNoEffectUnpublishShowForceAsync(ResolveShowReportNoEffectUnpublishShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastShowReportReviewSessions = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                        .Where(psrrs => psrrs.PodcastShowId == parameter.PodcastShowId)
                        .ToListAsync();

                    foreach (var session in podcastShowReportReviewSessions)
                    {
                        session.IsResolved = true;
                        await _podcastShowReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                        var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastShowId == session.PodcastShowId
                        && pbr.ResolvedAt == null)
                        .ToListAsync();
                        foreach (var ShowReport in podcastShowReportList)
                        {
                            ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
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
                    _logger.LogInformation("Successfully Resolve Show Report No Effect DMCA Remove Show Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Show Report No Effect Unpublish Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Show Report No Effect Unpublish Show Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Show Report No Effect Unpublish Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveChannelShowsReportNoEffectUnpublishChannelForceAsync(ResolveChannelShowsReportNoEffectUnpublishChannelForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    if (parameter.DmcaDismissedShowIds != null && parameter.DmcaDismissedShowIds.Count > 0)
                    {
                        foreach (var showId in parameter.DmcaDismissedShowIds)
                        {
                            var podcastShowReportReviewSessions = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                                .Where(psrrs => psrrs.PodcastShowId == showId)
                                .ToListAsync();

                            foreach (var session in podcastShowReportReviewSessions)
                            {
                                session.IsResolved = true;
                                await _podcastShowReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                                .Where(pbr => pbr.PodcastShowId == session.PodcastShowId
                                && pbr.ResolvedAt == null)
                                .ToListAsync();
                                foreach (var ShowReport in podcastShowReportList)
                                {
                                    ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
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
                    _logger.LogInformation("Successfully Resolve Channel Shows Report No Effect Unpublish Channel Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Channel Shows Report No Effect Unpublish Channel Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Channel Shows Report No Effect Unpublish Channel Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Channel Shows Report No Effect Unpublish Channel Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveShowReportNoEffectShowDeletionForceAsync(ResolveShowReportNoEffectShowDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastShowReportReviewSessions = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                        .Where(psrrs => psrrs.PodcastShowId == parameter.PodcastShowId)
                        .ToListAsync();

                    foreach (var session in podcastShowReportReviewSessions)
                    {
                        session.IsResolved = true;
                        await _podcastShowReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                        var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastShowId == session.PodcastShowId
                        && pbr.ResolvedAt == null)
                        .ToListAsync();
                        foreach (var ShowReport in podcastShowReportList)
                        {
                            ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
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
                    _logger.LogInformation("Successfully Resolve Show Report No Effect Show Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Show Report No Effect Show Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Show Report No Effect Show Deletion Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Show Report No Effect Show Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolveChannelShowsReportNoEffectChannelDeletionForceAsync(ResolveChannelShowsReportNoEffectChannelDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var showList = await GetPodcastShowByChannelId(parameter.PodcastChannelId);
                    if (showList != null)
                    {
                        foreach (var show in showList)
                        {
                            var podcastShowReportReviewSessions = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                                .Where(psrrs => psrrs.PodcastShowId == show.Id)
                                .ToListAsync();

                            foreach (var session in podcastShowReportReviewSessions)
                            {
                                session.IsResolved = true;
                                await _podcastShowReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                                .Where(pbr => pbr.PodcastShowId == session.PodcastShowId
                                && pbr.ResolvedAt == null)
                                .ToListAsync();
                                foreach (var ShowReport in podcastShowReportList)
                                {
                                    ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
                                }
                            }
                        }
                    } else
                    {
                        _logger.LogInformation("Unable to query Show for Podcast Channel Id: {PodcastChannelId}", parameter.PodcastChannelId);
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
                    _logger.LogInformation("Successfully Resolve Channel Shows Report No Effect Channel Deletion Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Channel Shows Report No Effect Channel Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Channel Shows Report No Effect Channel Deletion Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Channel Shows Report No Effect Channel Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolvePodcasterShowsReportNoEffectTerminatePodcasterForceAsync(ResolvePodcasterShowsReportNoEffectTerminatePodcasterForceParameterDTO parameter, SagaCommandMessage command)
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
                        foreach (var showId in showList.Select(s => s.Id))
                        {
                            var podcastShowReportReviewSessions = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                                .Where(psrrs => psrrs.PodcastShowId == showId)
                                .ToListAsync();

                            foreach (var session in podcastShowReportReviewSessions)
                            {
                                session.IsResolved = true;
                                await _podcastShowReportReviewSessionGenericRepository.UpdateAsync(session.Id, session);

                                var podcastShowReportList = await _podcastShowReportGenericRepository.FindAll()
                                .Where(pbr => pbr.PodcastShowId == session.PodcastShowId
                                && pbr.ResolvedAt == null)
                                .ToListAsync();
                                foreach (var ShowReport in podcastShowReportList)
                                {
                                    ShowReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastShowReportGenericRepository.UpdateAsync(ShowReport.Id, ShowReport);
                                }
                            }
                        }
                    } else
                    {
                        _logger.LogInformation("Unable to query Show for Podcaster Id: {PodcasterId}", parameter.PodcasterId);
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
                    _logger.LogInformation("Successfully Resolve Podcaster Shows Report No Effect Terminate Podcaster Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Podcaster Shows Report No Effect Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Podcaster Shows Report No Effect Terminate Podcaster Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Podcaster Shows Report No Effect Terminate Podcaster Force failed for SagaId: {SagaId}", command.SagaInstanceId);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while fetching Podcast Episode with Id: {PodcastEpisodeId}", podcastEpisodeId);
                throw new HttpRequestException("Query podcast episode with id failed. error: " + ex.Message);
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
            } catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while fetching Podcast Channel with Id: {PodcastChannelId}", podcastChannelId);
                throw new HttpRequestException("Query podcast channel with id failed. error: " + ex.Message);
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
                _logger.LogError(ex, "Error occurred while fetching Podcast Show with Id: {PodcastShowId}", podcastShowId);
                throw new HttpRequestException("Query podcast show with id failed. error: " + ex.Message);
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
                _logger.LogError(ex, "Error occurred while fetching Podcast Show with Channel Id: {PodcastChannelId}", podcastChannelid);
                throw new HttpRequestException("Query podcast show with channel id failed. error: " + ex.Message);
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
                _logger.LogError(ex, "Error occurred while fetching active System Config Profile from System Configuration Service");
                throw new HttpRequestException("Retreive active System Config Profile failed. Error: " + ex.Message);
            }
        }
        private async Task<int> GetRandomStaffFromList(List<AccountDTO>? array)
        {
            //if (array == null || array.Count == 0)
            //    return null;

            //var random = new Random();
            //var randomIndex = random.Next(array.Count);
            //return array[randomIndex];

            var assignedStaffIds = await _podcastShowReportReviewSessionGenericRepository.FindAll()
                .Where(psr => psr.IsResolved == null)
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
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                _logger.LogError(ex, "Error occurred while fetching Podcast Show with Podcaster Id: {PodcasterId}", podcasterId);
                throw new HttpRequestException("Query podcast show with podcaster id failed. error: " + ex.Message);
            }
        }
    }
}
