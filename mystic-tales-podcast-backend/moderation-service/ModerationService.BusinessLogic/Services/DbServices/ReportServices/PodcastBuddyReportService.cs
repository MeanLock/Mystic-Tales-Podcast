using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using ModerationService.BusinessLogic.DTOs.Account;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreatePodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveChannelEpisodesReportNoEffectChannelDeletionForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastBuddyReportNoEffectTerminatePodcasterForce;
using ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastShowReport;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.Details;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.ListItems;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport.Details;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport.ListItems;
using ModerationService.BusinessLogic.DTOs.Snippet;
using ModerationService.BusinessLogic.DTOs.SystemConfiguration;
using ModerationService.BusinessLogic.Enums.Account;
using ModerationService.BusinessLogic.Enums.Kafka;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.MessagingServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.DataAccess.Repositories.interfaces;
using ModerationService.Infrastructure.Models.Kafka;
using ModerationService.Infrastructure.Services.Kafka;
using Newtonsoft.Json.Linq;
using RazorLight.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ModerationService.BusinessLogic.Services.DbServices.ReportServices
{
    public class PodcastBuddyReportService
    {
        private readonly IGenericRepository<PodcastBuddyReport> _podcastBuddyReportGenericRepository;
        private readonly IGenericRepository<PodcastBuddyReportReviewSession> _podcastBuddyReportReviewSessionGenericRepository;
        private readonly IGenericRepository<PodcastBuddyReportType> _podcastBuddyReportTypeGenericRepository;

        private readonly AccountCachingService _accountCachingService;

        private readonly ILogger<PodcastBuddyReportService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public PodcastBuddyReportService(
            IGenericRepository<PodcastBuddyReport> podcastBuddyReportGenericRepository,
            IGenericRepository<PodcastBuddyReportReviewSession> podcastBuddyReportReviewSessionGenericRepository,
            IGenericRepository<PodcastBuddyReportType> podcastBuddyReportTypeGenericRepository,
            AccountCachingService accountCachingService,
            ILogger<PodcastBuddyReportService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper
            )
        {
            _podcastBuddyReportGenericRepository = podcastBuddyReportGenericRepository;
            _podcastBuddyReportReviewSessionGenericRepository = podcastBuddyReportReviewSessionGenericRepository;
            _podcastBuddyReportTypeGenericRepository = podcastBuddyReportTypeGenericRepository;
            _accountCachingService = accountCachingService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<List<PodcastBuddyReportListItemResponseDTO>> GetAllPodcastBuddyReportAsync()
        {
            try
            {
                var query = await _podcastBuddyReportGenericRepository.FindAll(
                    predicate: null,
                    includeFunc: source => source
                        .Include(r => r.PodcastBuddyReportType)
                    ).ToListAsync();
                var podcastBuddyReport = (await Task.WhenAll(query.Select(async pbr =>
                    {
                        var podcaster = await _accountCachingService.GetAccountStatusCacheById(pbr.PodcastBuddyId);
                        var account = await _accountCachingService.GetAccountStatusCacheById(pbr.AccountId);
                        return new PodcastBuddyReportListItemResponseDTO()
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
                            PodcastBuddy = new PodcastBuddySnippetResponseDTO()
                            {
                                Id = podcaster.Id,
                                FullName = podcaster.FullName,
                                Email = podcaster.Email,
                                MainImageFileKey = podcaster.MainImageFileKey
                            },
                            PodcastBuddyReportType = new PodcastBuddyReportTypeDTO()
                            {
                                Id = pbr.PodcastBuddyReportType.Id,
                                Name = pbr.PodcastBuddyReportType.Name,
                            },
                            ResolvedAt = pbr.ResolvedAt,
                            CreatedAt = pbr.CreatedAt,
                        };
                    }))).ToList();
                return podcastBuddyReport;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast buddy reports");
                throw new HttpRequestException("Retreive Buddy report failed. Error: " + ex.Message);
            }
        }
        public async Task CreatePodcastBuddyReportAsync(CreatePodcastBuddyReportParameterDTO parameter, SagaCommandMessage command)
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
                        throw new Exception("Active system configuration profile not found");
                    }

                    var podcaster = await ValidatePodcaster(parameter.PodcastBuddyId);
                    if (!podcaster.isValid)
                    {
                        throw new Exception(podcaster.errorMessage);
                    }

                    var existingReport = await _podcastBuddyReportGenericRepository.FindAll()
                        .Where(br => br.AccountId == parameter.AccountId && br.PodcastBuddyId == parameter.PodcastBuddyId && br.PodcastBuddyReportTypeId == parameter.PodcastBuddyReportTypeId && br.ResolvedAt == null)
                        .ToListAsync();
                    if (existingReport.Count() > 0)
                    {
                        throw new Exception("You have already reported this podcaster for the same reason and it is still unresolved");
                    }

                    var newBuddyReport = new PodcastBuddyReport()
                    {
                        AccountId = parameter.AccountId,
                        Content = parameter.Content,
                        PodcastBuddyId = parameter.PodcastBuddyId,
                        PodcastBuddyReportTypeId = parameter.PodcastBuddyReportTypeId,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };

                    var buddyReport = await _podcastBuddyReportGenericRepository.CreateAsync(newBuddyReport);

                    var existingBuddyReport = await _podcastBuddyReportGenericRepository.FindAll()
                        .Where(br => br.PodcastBuddyId == parameter.PodcastBuddyId && br.ResolvedAt == null)
                        .ToListAsync();
                    var existingBuddyReportReviewSession = await _podcastBuddyReportReviewSessionGenericRepository.FindAll()
                        .Where(brs => brs.PodcastBuddyId == parameter.PodcastBuddyId && brs.IsResolved == null)
                        .ToListAsync();
                    if(existingBuddyReportReviewSession.Count() == 0 || existingBuddyReportReviewSession == null)
                    {
                        if (existingBuddyReport.Count() >= systemConfig.ReviewSessionConfig.PodcastBuddyUnResolvedReportStreak)
                        {
                            var staffList = await GetStaffList();
                            if(staffList == null)
                            {
                                throw new Exception("Unable to query staff");
                            }
                            var randomStaff = await GetRandomStaffFromList(staffList);

                            var newBuddyReportReviewSession = new PodcastBuddyReportReviewSession()
                            {
                                AssignedStaff = randomStaff,
                                PodcastBuddyId = buddyReport.PodcastBuddyId,
                                IsResolved = null,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };

                            var buddyReportReviewSession = await _podcastBuddyReportReviewSessionGenericRepository.CreateAsync(newBuddyReportReviewSession);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "PodcastBuddyReportId", buddyReport.Id },
                        { "AccountId", buddyReport.AccountId },
                        { "PodcastBuddyId", buddyReport.PodcastBuddyId },
                        { "PodcastBuddyReportTypeId", buddyReport.PodcastBuddyReportTypeId },
                        { "CreatedAt", buddyReport.CreatedAt }
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
                    _logger.LogInformation("Successfully create podcast buddy report for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while create podcast buddy report for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create podcast buddy report failed, error: " + ex.Message }
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
                    _logger.LogInformation("Create podcast buddy report failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<List<PodcastBuddyReportTypeDTO>> GetAllPodcastBuddyReportTypeAsync()
        {
            try
            {
                var query = _podcastBuddyReportTypeGenericRepository.FindAll();
                return await query
                .Select(brt => new PodcastBuddyReportTypeDTO()
                {
                    Id = brt.Id,
                    Name = brt.Name
                })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast buddy report type review sessions");
                throw new HttpRequestException("Retreive Buddy report type failed. Error: " + ex.Message);

            }
        }
        public async Task<List<PodcastBuddyReportTypeDTO>> GetPodcastBuddyReportTypeAsync(AccountStatusCache? account, int podcastBuddyId)
        {
            try
            {
                var query = _podcastBuddyReportTypeGenericRepository.FindAll();
                if (account != null)
                {
                    var reportList = await _podcastBuddyReportGenericRepository.FindAll()
                    .Where(r => r.AccountId == account.Id && r.ResolvedAt == null && r.PodcastBuddyId == podcastBuddyId)
                    .Select(r => r.PodcastBuddyReportTypeId)
                    .Distinct()
                    .ToListAsync();
                    query = query.Where(brt => !reportList.Contains(brt.Id));
                }
                return await query
                .Select(brt => new PodcastBuddyReportTypeDTO()
                {
                    Id = brt.Id,
                    Name = brt.Name
                })
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast buddy report type review sessions");
                throw new HttpRequestException("Retreive Buddy report type failed. Error: " + ex.Message);

            }
        }
        public async Task<List<PodcastBuddyReportReviewSessionListItemResponseDTO>> GetBuddyReportReviewSessionAsync(int? staffId, int roleId)
        {
            try
            {
                var query = await _podcastBuddyReportReviewSessionGenericRepository.FindAll(
                    predicate: null
                    ).ToListAsync();
                if (roleId == (int)RoleEnum.Staff)
                {
                    query = query.Where(pbrrs => pbrrs.AssignedStaff == staffId).ToList();
                }
                var podcastBuddyReportReviewSession = (await Task.WhenAll(query.OrderByDescending(pbrrs => pbrrs.CreatedAt).Select(async pbrrs =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(pbrrs.PodcastBuddyId);
                    var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);
                    return new PodcastBuddyReportReviewSessionListItemResponseDTO()
                    {
                        Id = pbrrs.Id,
                        PodcastBuddy = new PodcastBuddySnippetResponseDTO()
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.FullName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        AssignedStaff = new AssignedStaffSnippetResponseDTO()
                        {
                            Id = staff.Id,
                            FullName = staff.FullName,
                            Email = staff.Email,
                            MainImageFileKey = staff.MainImageFileKey
                        },
                        ResolvedViolationPoint = pbrrs.ResolvedViolationPoint,
                        IsResolved = pbrrs.IsResolved,
                        CreatedAt = pbrrs.CreatedAt,
                        UpdatedAt = pbrrs.UpdatedAt,
                    };
                }))).ToList();
                return podcastBuddyReportReviewSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast buddy report review sessions");
                throw new HttpRequestException("Retreive Buddy report failed. Error: " + ex.Message);
            }
        }
        public async Task<PodcastBuddyReportReviewSessionDetailResponseDTO> GetBuddyReportReviewSessionByIdAsync(Guid id)
        {
            try
            {
                var pbrrs = await _podcastBuddyReportReviewSessionGenericRepository.FindByIdAsync(id);
                if (pbrrs == null)
                    return null;

                //if (pbrrs.IsResolved != null)
                //    throw new Exception("This buddy report review session has been resolved already");

                var query = await _podcastBuddyReportGenericRepository.FindAll(
                    predicate: null,
                    includeFunc: source => source
                        .Include(r => r.PodcastBuddyReportType)
                    )
                    .Where(r => r.PodcastBuddyId == pbrrs.PodcastBuddyId && r.ResolvedAt == null)
                    .ToListAsync();
                var podcastBuddyReport = (await Task.WhenAll(query.Select(async pbr =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(pbr.PodcastBuddyId);
                    var account = await _accountCachingService.GetAccountStatusCacheById(pbr.AccountId);
                    return new PodcastBuddyReportListItemResponseDTO()
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
                        PodcastBuddy = new PodcastBuddySnippetResponseDTO()
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.FullName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        PodcastBuddyReportType = new PodcastBuddyReportTypeDTO()
                        {
                            Id = pbr.PodcastBuddyReportType.Id,
                            Name = pbr.PodcastBuddyReportType.Name,
                        },
                        ResolvedAt = pbr.ResolvedAt,
                        CreatedAt = pbr.CreatedAt,
                    };
                }))).ToList();

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(pbrrs.PodcastBuddyId);
                var staff = await _accountCachingService.GetAccountStatusCacheById(pbrrs.AssignedStaff);

                return new PodcastBuddyReportReviewSessionDetailResponseDTO()
                {
                    Id = pbrrs.Id,
                    PodcastBuddy = new PodcastBuddySnippetResponseDTO()
                    {
                        Id = podcaster.Id,
                        FullName = podcaster.FullName,
                        Email = podcaster.Email,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    AssignedStaff = new AssignedStaffSnippetResponseDTO()
                    {
                        Id = staff.Id,
                        FullName = staff.FullName,
                        Email = staff.Email,
                        MainImageFileKey = staff.MainImageFileKey
                    },
                    ResolvedViolationPoint = pbrrs.ResolvedViolationPoint,
                    IsResolved = pbrrs.IsResolved,
                    CreatedAt = pbrrs.CreatedAt,
                    UpdatedAt = pbrrs.UpdatedAt,
                    BuddyReportList = pbrrs.IsResolved != null ? null : podcastBuddyReport
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching podcast buddy report review session by id");
                throw new HttpRequestException("Retreive Buddy report review session failed. Error: " + ex.Message);
            }
        }
        public async Task ResolvePodcastBuddyReportReviewSessionAsync(ResolvePodcastBuddyReportParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastBuddyReportReviewSessions = await _podcastBuddyReportReviewSessionGenericRepository.FindByIdAsync(parameter.PodcastBuddyReportReviewSessionId);
                    if (podcastBuddyReportReviewSessions.AssignedStaff != parameter.AccountId)
                    {
                        throw new Exception("The logged in Account is not authorize to resolve this buddy report review session");
                    }
                    podcastBuddyReportReviewSessions.ResolvedViolationPoint = parameter.ResolvedViolationPoint;
                    podcastBuddyReportReviewSessions.IsResolved = parameter.IsResolved;
                    await _podcastBuddyReportReviewSessionGenericRepository.UpdateAsync(podcastBuddyReportReviewSessions.Id, podcastBuddyReportReviewSessions);
                    var podcastBuddyReportList = await _podcastBuddyReportGenericRepository.FindAll()
                        .Where(pbr => pbr.PodcastBuddyId == podcastBuddyReportReviewSessions.PodcastBuddyId)
                        .ToListAsync();
                    foreach (var buddyReport in podcastBuddyReportList)
                    {
                        buddyReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastBuddyReportGenericRepository.UpdateAsync(buddyReport.Id, buddyReport);
                    }

                    if (parameter.IsTakenEffect && parameter.IsResolved)
                    {
                        var resolveRequestData = new JObject
                        {
                            { "AccountId", podcastBuddyReportReviewSessions.PodcastBuddyId },
                            { "ViolationPoint", podcastBuddyReportReviewSessions.ResolvedViolationPoint }
                        };
                        var resolveReportMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.UserManagementDomain,
                            requestData: resolveRequestData,
                            sagaInstanceId: null,
                            messageName: "user-violation-punishment-flow");
                        await _messagingService.SendSagaMessageAsync(resolveReportMessage, null);
                    }

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "IsResolved", podcastBuddyReportReviewSessions.IsResolved },
                        { "IsTakenEffect", parameter.IsTakenEffect },
                        { "ResolvedViolationPoint", podcastBuddyReportReviewSessions.ResolvedViolationPoint },
                        { "PodcastBuddyReportReviewSessionId", podcastBuddyReportReviewSessions.Id },
                        { "UpdatedAt", podcastBuddyReportReviewSessions.UpdatedAt }
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
                    _logger.LogInformation("Successfully resolve podcast buddy report review session for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while resolve podcast buddy report review session for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve podcast buddy report review session failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve podcast buddy report review session failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task ResolvePodcastBuddyReportNoEffectTerminatePodcasterForceAsync(ResolvePodcastBuddyReportNoEffectTerminatePodcasterForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastBuddyReportReviewSessions = await _podcastBuddyReportReviewSessionGenericRepository.FindAll()
                        .Where(pbrrs => pbrrs.PodcastBuddyId == parameter.PodcasterId && pbrrs.IsResolved == null)
                        .ToListAsync();
                    foreach (var reviewSession in podcastBuddyReportReviewSessions)
                    {
                        reviewSession.IsResolved = true;
                        await _podcastBuddyReportReviewSessionGenericRepository.UpdateAsync(reviewSession.Id, reviewSession);
                        var podcastBuddyReportList = await _podcastBuddyReportGenericRepository.FindAll()
                            .Where(pbr => pbr.PodcastBuddyId == reviewSession.PodcastBuddyId)
                            .ToListAsync();
                        foreach (var buddyReport in podcastBuddyReportList)
                        {
                            buddyReport.ResolvedAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastBuddyReportGenericRepository.UpdateAsync(buddyReport.Id, buddyReport);
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
                    _logger.LogInformation("Successfully Resolve Podcast Buddy Report No Effect Terminate Podcaster Force for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Resolve Podcast Buddy Report No Effect Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Resolve Podcast Buddy Report No Effect Terminate Podcaster Force failed, error: " + ex.Message }
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
                    _logger.LogInformation("Resolve Podcast Buddy Report No Effect Terminate Podcaster Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
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
                                        RoleId = (int)RoleEnum.Staff,
                                        DeactivatedAt = (DateTime?) null
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

            var assignedStaffIds = await _podcastBuddyReportReviewSessionGenericRepository.FindAll()
                .Where(pbr => pbr.IsResolved == null)
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
        private async Task<(bool isValid, string errorMessage)> ValidatePodcaster(int accountId)
        {
            var podcaster = await _accountCachingService.GetAccountStatusCacheById(accountId);
            if (podcaster == null)
            {
                return (false, "Podcaster not found");
            }

            if (podcaster.DeactivatedAt != null)
            {
                return (false, $"Podcaster with Id: {podcaster.Id} has already been deactivated");
            }

            if (!podcaster.HasVerifiedPodcasterProfile)
            {
                return (false, $"Podcaster with Id: {podcaster.Id} profile has not been verify");
            }
            return (true, string.Empty);
        }
    }
}
