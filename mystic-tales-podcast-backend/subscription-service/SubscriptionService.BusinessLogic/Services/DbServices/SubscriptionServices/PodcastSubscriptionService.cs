using GreenDonut;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.Account;
using SubscriptionService.BusinessLogic.DTOs.Cache;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.ActivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CAcceptPodcastSubscriptionNewestVersion;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelChannelSubscriptionChannelDeletionForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelChannelSubscriptionUnpublishChannelForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcasterChannelsSubscriptionTerminatePodcasterForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscriptionRegistration;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscriptionRegistrationChannelShows;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelShowSubscriptionDmcaRemoveShowForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelShowSubscriptionShowDeletionForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelShowSubscriptionUnpublishShowForce;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateAccountPodcastSubscriptionRegistration;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeactivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeletePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.UpdatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.Podcast;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.Details;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscriptionTransaction;
using SubscriptionService.BusinessLogic.DTOs.Snippet;
using SubscriptionService.BusinessLogic.DTOs.Subscription;
using SubscriptionService.BusinessLogic.DTOs.SystemConfiguration;
using SubscriptionService.BusinessLogic.DTOs.Transaction;
using SubscriptionService.BusinessLogic.Enums.Kafka;
using SubscriptionService.BusinessLogic.Enums.Podcast;
using SubscriptionService.BusinessLogic.Enums.Subscription;
using SubscriptionService.BusinessLogic.Enums.Transaction;
using SubscriptionService.BusinessLogic.Helpers.DateHelpers;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Models.Mail;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.DbServices.MiscServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities;
using SubscriptionService.DataAccess.Entities.SqlServer;
using SubscriptionService.DataAccess.Repositories.interfaces;
using SubscriptionService.Infrastructure.Models.Kafka;
using SubscriptionService.Infrastructure.Services.Kafka;
using System.Security.Principal;
using System.Threading.Channels;
using System.Transactions;

namespace SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices
{
    public class PodcastSubscriptionService
    {
        private readonly IGenericRepository<PodcastSubscription> _podcastSubscriptionGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionCycleTypePrice> _podcastSubscriptionCycleTypePriceGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionBenefitMapping> _podcastSubscriptionBenefitMappingGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionRegistration> _podcastSubscriptionRegistrationGenericRepository;

        private readonly AccountCachingService _accountCachingService;
        private readonly ILogger<PodcastSubscriptionService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public PodcastSubscriptionService(
            IGenericRepository<PodcastSubscription> podcastSubscriptionGenericRepository,
            IGenericRepository<PodcastSubscriptionCycleTypePrice> podcastSubscriptionCycleTypePriceGenericRepository,
            IGenericRepository<PodcastSubscriptionBenefitMapping> podcastSubscriptionBenefitMappingGenericRepository,
            IGenericRepository<PodcastSubscriptionRegistration> podcastSubscriptionRegistrationGenericRepository,
            AccountCachingService accountCachingService,
            ILogger<PodcastSubscriptionService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper)
        {
            _podcastSubscriptionGenericRepository = podcastSubscriptionGenericRepository;
            _podcastSubscriptionCycleTypePriceGenericRepository = podcastSubscriptionCycleTypePriceGenericRepository;
            _podcastSubscriptionBenefitMappingGenericRepository = podcastSubscriptionBenefitMappingGenericRepository;
            _podcastSubscriptionRegistrationGenericRepository = podcastSubscriptionRegistrationGenericRepository;
            _accountCachingService = accountCachingService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<List<PodcastSubscriptionListItemResponseDTO>> GetPodcastSubscriptionListByPodcastShowIdAsync(Guid podcastShowId)
        {
            var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                .Where(ps => ps.PodcastShowId == podcastShowId)
                .Select(ps => new PodcastSubscriptionListItemResponseDTO
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    Description = ps.Description,
                    PodcastShowId = ps.PodcastShowId,
                    PodcastChannelId = ps.PodcastChannelId,
                    IsActive = ps.IsActive,
                    CurrentVersion = ps.CurrentVersion,
                    DeletedAt = ps.DeletedAt,
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt
                })
                .ToListAsync();
            if (podcastSubscription == null)
            {
                _logger.LogWarning("No Podcast subscription with PodcastShow ID {PodcastShowId} found.", podcastShowId);
                return null;
            }
            return podcastSubscription;
        }
        public async Task<List<PodcastSubscriptionListItemResponseDTO>> GetPodcastSubscriptionListByPodcastChannelIdAsync(Guid podcastChannelId)
        {
            var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                .Where(ps => ps.PodcastChannelId == podcastChannelId)
                .Select(ps => new PodcastSubscriptionListItemResponseDTO
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    Description = ps.Description,
                    PodcastShowId = ps.PodcastShowId,
                    PodcastChannelId = ps.PodcastChannelId,
                    IsActive = ps.IsActive,
                    CurrentVersion = ps.CurrentVersion,
                    DeletedAt = ps.DeletedAt,
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt
                })
                .ToListAsync();
            if (podcastSubscription == null)
            {
                _logger.LogWarning("No Podcast subscription with PodcastChannel ID {PodcastChannelId} found.", podcastChannelId);
                return null;
            }
            return podcastSubscription;
        }
        public async Task CreatePodcastSubscriptionAsync(CreatePodcastSubscriptionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscription = new PodcastSubscription();
                    //var cycleTypePrices = new List<PodcastSubscriptionCycleTypePrice>();
                    //var benefitMappings = new List<PodcastSubscriptionBenefitMapping>();
                    PodcastShowDTO? show = null;
                    PodcastChannelDTO? channel = null;
                    if (parameter.PodcastShowId != null)
                    {
                        show = await GetPodcastShowWithAccountId(parameter.AccountId, parameter.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastShow with Id: {parameter.PodcastShowId}");
                        }
                        //var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        //    .FirstOrDefaultAsync(ps => ps.PodcastShowId == parameter.PodcastShowId && ps.DeletedAt == null);
                        //if (existPodcastSubscription != null)
                        //{
                        //    throw new Exception($"A Podcast Subscription already exists for PodcastShow Id: {parameter.PodcastShowId}");
                        //}
                        //var showValidation = await ValidateShow(parameter.PodcastShowId.Value);
                        //if (!showValidation.isValid)
                        //{
                        //    throw new Exception(showValidation.errorMessage);
                        //}
                    }
                    if (parameter.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, parameter.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastChanne; with Id: {parameter.PodcastChannelId}");
                        }
                        //var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        //    .FirstOrDefaultAsync(ps => ps.PodcastChannelId == parameter.PodcastChannelId && ps.DeletedAt == null);
                        //if (existPodcastSubscription != null)
                        //{
                        //    throw new Exception($"An Active Podcast Subscription already exists for PodcastChannel Id: {parameter.PodcastChannelId}");
                        //}
                        //var channelValidation = await ValidateChannel(parameter.PodcastChannelId.Value);
                        //if (!channelValidation.isValid)
                        //{
                        //    throw new Exception(channelValidation.errorMessage);
                        //}
                    }

                    //if(show.Count > 0 && show.HasValues)
                    //{
                    //    var status = show["PodcastShowStatusTracking"].OrderByDescending(x => x["CreatedAt"]).First();
                    //    // FIX: Properly convert JToken to int for comparison
                    //    int podcastShowStatusId = status.ToObject<PodcastShowStatusTrackingListItemDTO>().PodcastShowStatusId;
                    //    if (podcastShowStatusId != (int)PodcastShowStatusEnum.ReadyToRelease && podcastShowStatusId != (int)PodcastShowStatusEnum.Published)
                    //    {
                    //        throw new Exception($"Podcast Show with Id: {parameter.PodcastShowId} is not elligle for creating subscription");
                    //    }
                    //}
                    //if(channel.Count > 0 && channel.HasValues)
                    //{
                    //    var status = channel["PodcastChannelStatusTracking"].OrderByDescending(x => x["CreatedAt"]).First();
                    //    // FIX: Properly convert JToken to int for comparison
                    //    int podcastChannelStatusId = status.ToObject<PodcastChannelStatusTrackingListItemDTO>().PodcastChannelStatusId;
                    //    if (podcastChannelStatusId != (int)PodcastChannelStatusEnum.Published)
                    //    {
                    //        throw new Exception($"Podcast Channel with Id: {parameter.PodcastChannelId} is not elligle for creating subscription");
                    //    }
                    //}

                    var newPodcastSubscription = new PodcastSubscription
                    {
                        Name = parameter.Name,
                        Description = parameter.Description,
                        PodcastShowId = parameter.PodcastShowId,
                        PodcastChannelId = parameter.PodcastChannelId,
                        CurrentVersion = 1,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    podcastSubscription = await _podcastSubscriptionGenericRepository.CreateAsync(newPodcastSubscription);
                    foreach (var cycleTypePrice in parameter.PodcastSubscriptionCycleTypePriceList)
                    {
                        var newCycleTypePrice = new PodcastSubscriptionCycleTypePrice
                        {
                            PodcastSubscriptionId = podcastSubscription.Id,
                            SubscriptionCycleTypeId = cycleTypePrice.SubscriptionCycleTypeId,
                            Version = 1,
                            Price = cycleTypePrice.Price,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var cycleTypePriceResult = await _podcastSubscriptionCycleTypePriceGenericRepository.CreateAsync(newCycleTypePrice);
                        //cycleTypePrices.Add(cycleTypePriceResult);
                    }
                    foreach (var benefitId in parameter.PodcastSubscriptionBenefitMappingList)
                    {
                        var newBenefitMapping = new PodcastSubscriptionBenefitMapping
                        {
                            PodcastSubscriptionId = podcastSubscription.Id,
                            PodcastSubscriptionBenefitId = benefitId,
                            Version = 1,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var benefitMappingResult = await _podcastSubscriptionBenefitMappingGenericRepository.CreateAsync(newBenefitMapping);
                        //if (benefitMappings == null)
                        //{
                        //    benefitMappings.Add(benefitMappingResult);
                        //}
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "PodcastSubscriptionId", podcastSubscription.Id },
                        { "Name", podcastSubscription.Name },
                        { "Description", podcastSubscription.Description },
                        { "PodcastShowId", podcastSubscription.PodcastShowId },
                        { "PodcastChannelId", podcastSubscription.PodcastChannelId },
                        //{ "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(cycleTypePrices) },
                        //{ "PodcastSubscriptionBenefitMappingList", JArray.FromObject(benefitMappings) },
                        { "CreatedAt", podcastSubscription.CreatedAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully created podcast subscription for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating podcast subscription for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create podcast subscription failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Create podcast subscription failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<PodcastSubscriptionDetailResponseDTO?> GetPodcastSubscriptionByIdAsync(bool isPodcaster, int podcastSubscriptionId)
        {
            var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll(
                includeFunc: ps => ps
                    .Include(sct => sct.PodcastSubscriptionCycleTypePrices)
                    .ThenInclude(sct => sct.SubscriptionCycleType)
                    .Include(bm => bm.PodcastSubscriptionBenefitMappings)
                    .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                    .Include(sr => sr.PodcastSubscriptionRegistrations)
                    .ThenInclude(sr => sr.SubscriptionCycleType)
                )
                .Where(ps => ps.Id == podcastSubscriptionId)
                .FirstOrDefaultAsync();

            if (podcastSubscription == null)
            {
                _logger.LogWarning("No Podcast subscription with ID {PodcastSubscriptionId} found.", podcastSubscriptionId);
                return null;
            }

            // Handle account caching separately after the main query
            List<PodcastSubscriptionRegistrationListItemResponseDTO>? registrationList = null;
            
            if (isPodcaster && podcastSubscription.PodcastSubscriptionRegistrations?.Any() == true)
            {
                // Get all unique account IDs first
                var accountIds = podcastSubscription.PodcastSubscriptionRegistrations
                    .Select(sr => sr.AccountId)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();

                // Batch fetch accounts (more efficient)
                var accountCache = new Dictionary<int, AccountStatusCache>();
                foreach (var accountId in accountIds)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(accountId);
                    if (account != null)
                    {
                        accountCache[accountId] = account;
                    }
                }

                // Map registrations with cached accounts
                registrationList = podcastSubscription.PodcastSubscriptionRegistrations
                    .Select(sr => new PodcastSubscriptionRegistrationListItemResponseDTO
                    {
                        Id = sr.Id,
                        Account = sr.AccountId.HasValue && accountCache.ContainsKey(sr.AccountId.Value) 
                            ? new AccountSnippetResponseDTO
                            {
                                Id = accountCache[sr.AccountId.Value].Id,
                                Email = accountCache[sr.AccountId.Value].Email,
                                FullName = accountCache[sr.AccountId.Value].FullName,
                                MainImageFileKey = accountCache[sr.AccountId.Value].MainImageFileKey
                            }
                            : null,
                        PodcastSubscriptionId = sr.PodcastSubscriptionId,
                        SubscriptionCycleType = sr.SubscriptionCycleType == null
                            ? null
                            : new SubscriptionCycleTypeDTO
                            {
                                Id = sr.SubscriptionCycleType.Id,
                                Name = sr.SubscriptionCycleType.Name
                            },
                        CurrentVersion = sr.CurrentVersion,
                        IsAcceptNewestVersionSwitch = sr.IsAcceptNewestVersionSwitch,
                        IsIncomeTaken = sr.IsIncomeTaken,
                        LastPaidAt = sr.LastPaidAt,
                        CancelledAt = sr.CancelledAt,
                        CreatedAt = sr.CreatedAt,
                        UpdatedAt = sr.UpdatedAt
                    }).ToList();
            }

            // Create the response DTO
            var result = new PodcastSubscriptionDetailResponseDTO
            {
                Id = podcastSubscription.Id,
                Name = podcastSubscription.Name,
                Description = podcastSubscription.Description,
                PodcastShowId = podcastSubscription.PodcastShowId,
                PodcastChannelId = podcastSubscription.PodcastChannelId,
                IsActive = podcastSubscription.IsActive,
                CurrentVersion = podcastSubscription.CurrentVersion,
                DeletedAt = podcastSubscription.DeletedAt,
                CreatedAt = podcastSubscription.CreatedAt,
                UpdatedAt = podcastSubscription.UpdatedAt,
                PodcastSubscriptionCycleTypePriceList = podcastSubscription.PodcastSubscriptionCycleTypePrices
                    .Select(ctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                    {
                        PodcastSubscriptionId = ctp.PodcastSubscriptionId,
                        SubscriptionCycleType = ctp.SubscriptionCycleType == null
                            ? null
                            : new SubscriptionCycleTypeDTO
                            {
                                Id = ctp.SubscriptionCycleType.Id,
                                Name = ctp.SubscriptionCycleType.Name
                            },
                        Version = ctp.Version,
                        Price = ctp.Price,
                        CreatedAt = ctp.CreatedAt,
                        UpdatedAt = ctp.UpdatedAt
                    }).ToList(),
                PodcastSubscriptionBenefitMappingList = podcastSubscription.PodcastSubscriptionBenefitMappings
                    .Select(bm => new PodcastSubscriptionBenefitMappingListItemResponseDTO
                    {
                        PodcastSubscriptionId = bm.PodcastSubscriptionId,
                        PodcastSubscriptionBenefit = bm.PodcastSubscriptionBenefit == null
                            ? null
                            : new PodcastSubscriptionBenefitDTO
                            {
                                Id = bm.PodcastSubscriptionBenefit.Id,
                                Name = bm.PodcastSubscriptionBenefit.Name
                            },
                        Version = bm.Version,
                        CreatedAt = bm.CreatedAt,
                        UpdatedAt = bm.UpdatedAt
                    }).ToList(),
                PodcastSubscriptionRegistrationList = registrationList
            };

            return result;
        }
        public async Task UpdatePodcastSubscriptionAsync(UpdatePodcastSubscriptionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscription = null as PodcastSubscription;
                    //var cycleTypePrices = null as List<PodcastSubscriptionCycleTypePrice>;
                    //var benefitMappings = null as List<PodcastSubscriptionBenefitMapping>;
                    var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.DeletedAt == null);
                    if (existPodcastSubscription == null)
                    {
                        _logger.LogWarning("No Active Podcast Subscription exists for PodcastSubscription Id: {PodcastSubscriptionId}", parameter.PodcastSubscriptionId);
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }

                    PodcastShowDTO? show = null;
                    PodcastChannelDTO? channel = null;
                    if (existPodcastSubscription.PodcastShowId != null)
                    {
                        show = await GetPodcastShowWithAccountId(parameter.AccountId, existPodcastSubscription.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to update PodcastSubscription for PodcastShow with Id: {existPodcastSubscription.PodcastShowId}");
                        }
                        //var showValidation = await ValidateShow(existPodcastSubscription.PodcastShowId.Value);
                        //if (!showValidation.isValid)
                        //{
                        //    throw new Exception(showValidation.errorMessage);
                        //}
                    }
                    if (existPodcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, existPodcastSubscription.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to update PodcastSubscription for PodcastChannel with Id: {existPodcastSubscription.PodcastChannelId}");
                        }
                        //var channelValidation = await ValidateChannel(existPodcastSubscription.PodcastChannelId.Value);
                        //if (!channelValidation.isValid)
                        //{
                        //    throw new Exception(channelValidation.errorMessage);
                        //}
                    }

                    //if (show.Count > 0 && show.HasValues)
                    //{
                    //    var status = show["PodcastShowStatusTracking"].OrderByDescending(x => x["CreatedAt"]).First();
                    //    // FIX: Properly convert JToken to int for comparison
                    //    int podcastShowStatusId = status.ToObject<PodcastShowStatusTrackingListItemDTO>().PodcastShowStatusId;
                    //    if (podcastShowStatusId != (int)PodcastShowStatusEnum.ReadyToRelease && podcastShowStatusId != (int)PodcastShowStatusEnum.Published)
                    //    {
                    //        throw new Exception($"Podcast Show with Id: {podcastSubscription.PodcastShowId} is not elligle for creating subscription");
                    //    }
                    //}
                    //if (channel.Count > 0 && channel.HasValues)
                    //{
                    //    var status = channel["PodcastChannelStatusTracking"].OrderByDescending(x => x["CreatedAt"]).First();
                    //    // FIX: Properly convert JToken to int for comparison
                    //    int podcastChannelStatusId = status.ToObject<PodcastChannelStatusTrackingListItemDTO>().PodcastChannelStatusId;
                    //    if (podcastChannelStatusId != (int)PodcastChannelStatusEnum.Published)
                    //    {
                    //        throw new Exception($"Podcast Channel with Id: {podcastSubscription.PodcastChannelId} is not elligle for creating subscription");
                    //    }
                    //}

                    existPodcastSubscription.Name = parameter.Name;
                    existPodcastSubscription.Description = parameter.Description;
                    existPodcastSubscription.CurrentVersion += 1;
                    existPodcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();

                    podcastSubscription = await _podcastSubscriptionGenericRepository.UpdateAsync(existPodcastSubscription.Id, existPodcastSubscription);
                    foreach (var cycleTypePrice in parameter.PodcastSubscriptionCycleTypePriceList)
                    {
                        var newCycleTypePrice = new PodcastSubscriptionCycleTypePrice
                        {
                            PodcastSubscriptionId = podcastSubscription.Id,
                            SubscriptionCycleTypeId = cycleTypePrice.SubscriptionCycleTypeId,
                            Version = podcastSubscription.CurrentVersion,
                            Price = cycleTypePrice.Price,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var cycleTypePriceResult = await _podcastSubscriptionCycleTypePriceGenericRepository.CreateAsync(newCycleTypePrice);
                        //if (cycleTypePrices == null)
                        //{
                        //    cycleTypePrices.Add(cycleTypePriceResult);
                        //}
                    }
                    foreach (var benefitId in parameter.PodcastSubscriptionBenefitMappingList)
                    {
                        var newBenefitMapping = new PodcastSubscriptionBenefitMapping
                        {
                            PodcastSubscriptionId = podcastSubscription.Id,
                            PodcastSubscriptionBenefitId = benefitId,
                            Version = podcastSubscription.CurrentVersion,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var benefitMappingResult = await _podcastSubscriptionBenefitMappingGenericRepository.CreateAsync(newBenefitMapping);
                        //if (benefitMappings == null)
                        //{
                        //    benefitMappings.Add(benefitMappingResult);
                        //}
                    }

                    var updatedSubscription = new PodcastSubscriptionListItemResponseDTO
                    {
                        Id = podcastSubscription.Id,
                        Name = podcastSubscription.Name,
                        Description = podcastSubscription.Description,
                        PodcastShowId = podcastSubscription.PodcastShowId,
                        PodcastChannelId = podcastSubscription.PodcastChannelId,
                        IsActive = podcastSubscription.IsActive,
                        CurrentVersion = podcastSubscription.CurrentVersion,
                        DeletedAt = podcastSubscription.DeletedAt,
                        CreatedAt = podcastSubscription.CreatedAt,
                        UpdatedAt = podcastSubscription.UpdatedAt
                    };
                    var updateMappingList = await _podcastSubscriptionBenefitMappingGenericRepository.FindAll()
                        .Where(bm => bm.PodcastSubscriptionId == podcastSubscription.Id && bm.Version == podcastSubscription.CurrentVersion)
                        .Select(bm => new PodcastSubscriptionBenefitMappingListItemResponseDTO
                        {
                            PodcastSubscriptionId = bm.PodcastSubscriptionId,
                            PodcastSubscriptionBenefit = new PodcastSubscriptionBenefitDTO
                            {
                                Id = bm.PodcastSubscriptionBenefit.Id,
                                Name = bm.PodcastSubscriptionBenefit.Name
                            },
                            Version = bm.Version,
                            CreatedAt = bm.CreatedAt,
                            UpdatedAt = bm.UpdatedAt
                        })
                        .ToListAsync();
                    var updateCycleList = await _podcastSubscriptionCycleTypePriceGenericRepository.FindAll()
                        .Where(ctp => ctp.PodcastSubscriptionId == podcastSubscription.Id && ctp.Version == podcastSubscription.CurrentVersion)
                        .Select(ctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                        {
                            PodcastSubscriptionId = ctp.PodcastSubscriptionId,
                            SubscriptionCycleType = new SubscriptionCycleTypeDTO
                            {
                                Id = ctp.SubscriptionCycleType.Id,
                                Name = ctp.SubscriptionCycleType.Name
                            },
                            Version = ctp.Version,
                            Price = ctp.Price,
                            CreatedAt = ctp.CreatedAt,
                            UpdatedAt = ctp.UpdatedAt
                        })
                        .ToListAsync();

                    var updateRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(sr => sr.PodcastSubscriptionId == parameter.PodcastSubscriptionId && sr.CancelledAt == null)
                        .ToListAsync();

                    foreach (var registration in updateRegistrations)
                    {
                        registration.IsAcceptNewestVersionSwitch = false;
                        registration.UpdatedAt = _dateHelper.GetNowByAppTimeZone();

                        var account = await _accountCachingService.GetAccountStatusCacheById(registration.AccountId.Value);
                        var newVersionMailSendingRequestData = JObject.FromObject(new
                        {
                            SendSubscriptionServiceEmailInfo = new
                            {
                                MailTypeName = "PodcastSubscriptionNewVersion",
                                ToEmail = account.Email,
                                MailObject = new PodcastSubscriptionNewVersionMailViewModel
                                {
                                    CustomerFullName = account.FullName,
                                    PodcastSubscription = updatedSubscription,
                                    PodcastSubscriptionCycleTypePriceList = updateCycleList,
                                    PodcastSubscriptionBenefitList = updateMappingList,
                                    CreatedDate = _dateHelper.GetNowByAppTimeZone()
                                }
                            }
                        });
                        var customerMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.SubscriptionManagementDomain,
                            requestData: newVersionMailSendingRequestData,
                            sagaInstanceId: null,
                            messageName: "subscription-service-mail-sending-flow");
                        await _messagingService.SendSagaMessageAsync(customerMailSendingFlow);

                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);
                        
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "PodcastSubscriptionId", podcastSubscription.Id },
                        { "Name", podcastSubscription.Name },
                        { "Description", podcastSubscription.Description },
                        //{ "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(cycleTypePrices) },
                        //{ "PodcastSubscriptionBenefitMappingList", JArray.FromObject(benefitMappings) },
                        { "UpdatedAt", podcastSubscription.UpdatedAt },
                        { "NewVersion", podcastSubscription.CurrentVersion }
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully updated podcast subscription for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while updating podcast subscription for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Update podcast subscription failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Update podcast subscription failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DeletePodcastSubscriptionAsync(DeletePodcastSubscriptionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.DeletedAt == null);
                    PodcastShowDTO? show = null;
                    PodcastChannelDTO? channel = null;
                    if (existPodcastSubscription == null)
                    {
                        _logger.LogWarning("No Active Podcast Subscription exists for PodcastSubscription Id: {PodcastSubscriptionId}", parameter.PodcastSubscriptionId);
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }
                    if (existPodcastSubscription.PodcastShowId != null)
                    {
                        show = await GetPodcastShowWithAccountId(parameter.AccountId, existPodcastSubscription.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to delete PodcastSubscription for PodcastShow with Id: {existPodcastSubscription.PodcastShowId}");
                        }
                        //var showValidation = await ValidateShow(existPodcastSubscription.PodcastShowId.Value);
                        //if (!showValidation.isValid)
                        //{
                        //    throw new Exception(showValidation.errorMessage);
                        //}
                    }
                    if (existPodcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, existPodcastSubscription.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to delete PodcastSubscription for PodcastChannel with Id: {existPodcastSubscription.PodcastChannelId}");
                        }
                        //var channelValidation = await ValidateChannel(existPodcastSubscription.PodcastChannelId.Value);
                        //if (!channelValidation.isValid)
                        //{
                        //    throw new Exception(channelValidation.errorMessage);
                        //}
                    }

                    //if (show.Count > 0 && show.HasValues)
                    //{
                    //    var status = show["PodcastShowStatusTracking"].OrderByDescending(x => x["CreatedAt"]).First();
                    //    // FIX: Properly convert JToken to int for comparison
                    //    int podcastShowStatusId = status.ToObject<PodcastShowStatusTrackingListItemDTO>().PodcastShowStatusId;
                    //    if (podcastShowStatusId != (int)PodcastShowStatusEnum.ReadyToRelease && podcastShowStatusId != (int)PodcastShowStatusEnum.Published)
                    //    {
                    //        throw new Exception($"Podcast Show with Id: {existPodcastSubscription.PodcastShowId} is not elligle for creating subscription");
                    //    }
                    //}
                    //if (channel.Count > 0 && channel.HasValues)
                    //{
                    //    var status = channel["PodcastChannelStatusTracking"].OrderByDescending(x => x["CreatedAt"]).First();
                    //    // FIX: Properly convert JToken to int for comparison
                    //    int podcastChannelStatusId = status.ToObject<PodcastChannelStatusTrackingListItemDTO>().PodcastChannelStatusId;
                    //    if (podcastChannelStatusId != (int)PodcastChannelStatusEnum.Published)
                    //    {
                    //        throw new Exception($"Podcast Channel with Id: {existPodcastSubscription.PodcastChannelId} is not elligle for creating subscription");
                    //    }
                    //}

                    existPodcastSubscription.IsActive = false;
                    existPodcastSubscription.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                    existPodcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _podcastSubscriptionGenericRepository.UpdateAsync(existPodcastSubscription.Id, existPodcastSubscription);

                    var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(sr => sr.PodcastSubscriptionId == existPodcastSubscription.Id && sr.CancelledAt == null)
                        .ToListAsync();
                    foreach (var registration in subscriptionRegistrations)
                    {
                        if (!registration.IsIncomeTaken)
                        {
                            var amount = existPodcastSubscription.PodcastSubscriptionCycleTypePrices
                                .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                .Select(ptcp => ptcp.Price)
                                .FirstOrDefault();
                            var tempRequestData = new JObject
                            {
                                { "PodcastSubscriptionRegistrationId", registration.Id },
                                { "Profit", null },
                                { "AccountId", registration.AccountId },
                                { "Amount", amount },
                                { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                            };
                            var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: tempRequestData,
                                sagaInstanceId: null,
                                messageName: "podcast-subscription-refund-flow");
                            await _messagingService.SendSagaMessageAsync(refundMessage, null);
                        }
                        await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "PodcastSubscriptionId", existPodcastSubscription.Id },
                        { "DeletedAt", existPodcastSubscription.DeletedAt }
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully deleted podcast subscription for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deleting podcast subscription for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Delete podcast subscription failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Delete podcast subscription failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CreateAccountPodcastSubscriptionRegistrationAsync(CreateAccountPodcastSubscriptionRegistrationParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices))
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.DeletedAt == null && ps.IsActive);
                    if (podcastSubscription == null)
                    {
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }

                    var account = await _accountCachingService.GetAccountStatusCacheById(parameter.AccountId);

                    if (podcastSubscription.PodcastShowId != null)
                    {
                        var showValidation = await ValidateShow(podcastSubscription.PodcastShowId.Value);
                        if (!showValidation.isValid)
                        {
                            throw new Exception(showValidation.errorMessage);
                        }
                    }

                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        var channelValidation = await ValidateChannel(podcastSubscription.PodcastChannelId.Value);
                        if (!channelValidation.isValid)
                        {
                            throw new Exception(channelValidation.errorMessage);
                        }
                    }

                    var existRegistration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .FirstOrDefaultAsync(psr => psr.AccountId == parameter.AccountId
                            && psr.PodcastSubscriptionId == parameter.PodcastSubscriptionId
                            && psr.CancelledAt == null);
                    if (existRegistration != null)
                    {
                        throw new Exception($"An Active Podcast Subscription Registration exists for Account Id: {parameter.AccountId} and PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }

                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        var listShow = await GetPodcastShowByPodcastChannelId(podcastSubscription.PodcastChannelId.Value);
                        if (listShow != null && listShow.Count > 0)
                        {
                            var listRegistration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Where(psr => psr.AccountId == parameter.AccountId
                                    && listShow.Select(s => s.Id).Contains(psr.PodcastSubscription.PodcastShowId ?? Guid.Empty)
                                    && psr.CancelledAt == null)
                                .ToListAsync();

                            if (listRegistration != null && listRegistration.Count > 0)
                            {
                                foreach (var podcastSubscriptionRegistration in listRegistration)
                                {
                                    var systemConfig = await GetActiveSystemConfigProfile();
                                    var profitRate = systemConfig.PodcastSubscriptionConfigs
                                        .Where(ps => ps.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                                        .Select(psc => psc.ProfitRate)
                                        .FirstOrDefault();
                                    var incomeTakenDelayDays = systemConfig.PodcastSubscriptionConfigs
                                        .Where(psc => psc.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                                        .Select(psc => psc.IncomeTakenDelayDays)
                                        .FirstOrDefault();

                                    decimal refundAmount = 0;
                                    if (podcastSubscriptionRegistration.LastPaidAt.AddDays((double)incomeTakenDelayDays) < _dateHelper.GetNowByAppTimeZone())
                                    {
                                        podcastSubscriptionRegistration.IsIncomeTaken = true;
                                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(podcastSubscriptionRegistration.Id, podcastSubscriptionRegistration);

                                        var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                                            .Select(ptcp => ptcp.Price)
                                            .FirstOrDefault();

                                        var amount = originalPrice - originalPrice * (decimal)profitRate;
                                        var profit = originalPrice * (decimal)profitRate;

                                        var transactionRequestData = new JObject
                                        {
                                            { "PodcastSubscriptionRegistrationId", podcastSubscriptionRegistration.Id },
                                            { "Profit", profit },
                                            { "AccountId", podcastSubscriptionRegistration.AccountId },
                                            { "Amount", amount },
                                            { "TransactionTypeId", (int)TransactionTypeEnum.PodcasterSubscriptionIncome }
                                        };
                                        var transactionMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.PaymentProcessingDomain,
                                            requestData: transactionRequestData,
                                            sagaInstanceId: null,
                                            messageName: "podcaster-subscription-income-release-flow");
                                        await _messagingService.SendSagaMessageAsync(transactionMessage, null);
                                    }
                                    else
                                    {
                                        var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                                            .Select(ptcp => ptcp.Price)
                                            .FirstOrDefault();
                                        refundAmount = originalPrice;
                                        var transactionRequestData = new JObject
                                        {
                                            { "PodcastSubscriptionRegistrationId", podcastSubscriptionRegistration.Id },
                                            { "Profit", null },
                                            { "AccountId", podcastSubscriptionRegistration.AccountId },
                                            { "Amount", originalPrice },
                                            { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                        };
                                        var transactionMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.PaymentProcessingDomain,
                                            requestData: transactionRequestData,
                                            sagaInstanceId: null,
                                            messageName: "podcast-subscription-refund-flow");
                                        await _messagingService.SendSagaMessageAsync(transactionMessage, null);
                                    }
                                    
                                    var duplicateMailSendingRequestData = JObject.FromObject(new
                                    {
                                        SendSubscriptionServiceEmailInfo = new
                                        {
                                            MailTypeName = "PodcastSubscriptionDuplicate",
                                            ToEmail = account.Email,
                                            MailObject = new PodcastSubscriptionDuplicateMailViewModel
                                            {
                                                CustomerFullName = account.FullName,
                                                RefundAmount = refundAmount,
                                                CancelledDate = _dateHelper.GetNowByAppTimeZone()
                                            }
                                        }
                                    });
                                    var customerMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                        requestData: duplicateMailSendingRequestData,
                                        sagaInstanceId: null,
                                        messageName: "subscription-service-mail-sending-flow");
                                    await _messagingService.SendSagaMessageAsync(customerMailSendingFlow);

                                    podcastSubscriptionRegistration.CancelledAt = _dateHelper.GetNowByAppTimeZone();
                                    await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(podcastSubscriptionRegistration.Id, podcastSubscriptionRegistration);
                                }
                            }
                        }
                    }
                    else if (podcastSubscription.PodcastShowId != null)
                    {
                        var show = await GetPodcastShow(podcastSubscription.PodcastShowId.Value);
                        if(show.PodcastChannelId != null)
                        {
                            var existSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                                .FirstOrDefaultAsync(ps => ps.PodcastChannelId == show.PodcastChannelId && ps.DeletedAt == null && ps.IsActive);
                            if (existSubscription != null)
                            {
                                throw new HttpRequestException($"An Active Podcast Subscription exists for Podcast Channel Id: {show.PodcastChannelId}. Please subscribe to the channel subscription instead.");
                            }
                        }
                    }

                    var newRegistration = new PodcastSubscriptionRegistration
                    {
                        AccountId = parameter.AccountId,
                        PodcastSubscriptionId = parameter.PodcastSubscriptionId,
                        SubscriptionCycleTypeId = parameter.SubscriptionCycleTypeId,
                        CurrentVersion = podcastSubscription.CurrentVersion,
                        IsAcceptNewestVersionSwitch = null,
                        IsIncomeTaken = false,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    var registrationResult = await _podcastSubscriptionRegistrationGenericRepository.CreateAsync(newRegistration);

                    var originalPrice2 = podcastSubscription.PodcastSubscriptionCycleTypePrices
                        .Where(ptcp => ptcp.SubscriptionCycleTypeId == parameter.SubscriptionCycleTypeId)
                        .Select(ptcp => ptcp.Price)
                        .FirstOrDefault();

                    var transactionRequestData2 = new JObject
                    {
                        { "PodcastSubscriptionRegistrationId", newRegistration.Id },
                        { "Profit", null },
                        { "AccountId", parameter.AccountId },
                        { "Amount", originalPrice2 },
                        { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePayment }
                    };
                    var transactionMessage2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: transactionRequestData2,
                        sagaInstanceId: null,
                        messageName: "podcast-subscription-payment-flow");
                    await _messagingService.SendSagaMessageAsync(transactionMessage2, null);

                    var registrationMailSendingRequestData = JObject.FromObject(new
                    {
                        SendSubscriptionServiceEmailInfo = new
                        {
                            MailTypeName = "PodcastSubscriptionRegistration",
                            ToEmail = account.Email,
                            MailObject = new PodcastSubscriptionRegistrationMailViewModel
                            {
                                CustomerFullName = account.FullName,
                                Price = originalPrice2,
                                CreatedDate = _dateHelper.GetNowByAppTimeZone()
                            }
                        }
                    });
                    var customerMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: registrationMailSendingRequestData,
                        sagaInstanceId: null,
                        messageName: "subscription-service-mail-sending-flow");
                    await _messagingService.SendSagaMessageAsync(customerMailSendingFlow2);

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                        {
                            { "PodcastSubscriptionRegistrationId", registrationResult.Id },
                            { "AccountId", registrationResult.AccountId },
                            { "PodcastSubscriptionId", registrationResult.PodcastSubscriptionId },
                            { "SubscriptionCycleTypeId", registrationResult.SubscriptionCycleTypeId },
                            { "CreatedAt", registrationResult.CreatedAt }
                        };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully created podcast subscription registration for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating podcast subscription registration for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create podcast subscription registration failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Create podcast subscription registration failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<List<PodcastSubscriptionRegistrationListItemResponseDTO>> GetChannelPodcastSubscriptionsRegistrationsByAccountIdAsync(int accountId)
        {
            var podcastSubscriptionsRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll(
                includeFunc: psr => psr
                    .Include(ps => ps.PodcastSubscription)
                    .Include(sct => sct.SubscriptionCycleType)
                )
                .Where(psr => psr.AccountId == accountId && psr.CancelledAt == null && psr.PodcastSubscription.PodcastChannelId != null)
                .ToListAsync();
            List<PodcastSubscriptionRegistrationListItemResponseDTO> result = new();
            foreach (var psr in podcastSubscriptionsRegistrations)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(psr.AccountId.Value);
                result.Add(new PodcastSubscriptionRegistrationListItemResponseDTO
                {
                    Id = psr.Id,
                    Account = new AccountSnippetResponseDTO
                    {
                        Id = account.Id,
                        Email = account.Email,
                        FullName = account.FullName,
                        MainImageFileKey = account.MainImageFileKey
                    },
                    PodcastSubscriptionId = psr.PodcastSubscriptionId,
                    SubscriptionCycleType = psr.SubscriptionCycleType == null
                    ? null
                    : new SubscriptionCycleTypeDTO
                    {
                        Id = psr.SubscriptionCycleType.Id,
                        Name = psr.SubscriptionCycleType.Name
                    },
                    CurrentVersion = psr.CurrentVersion,
                    IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                    IsIncomeTaken = psr.IsIncomeTaken,
                    LastPaidAt = psr.LastPaidAt,
                    CancelledAt = psr.CancelledAt,
                    CreatedAt = psr.CreatedAt,
                    UpdatedAt = psr.UpdatedAt
                });
            };
                
            if (result == null || result.Count == 0)
            {
                _logger.LogWarning("No Podcast subscription registrations found for Account ID {AccountId}.", accountId);
                return null;
            }
            return result;
        }
        public async Task<List<PodcastSubscriptionRegistrationListItemResponseDTO>> GetShowPodcastSubscriptionsRegistrationsByAccountIdAsync(int accountId)
        {
            var podcastSubscriptionsRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll(
                includeFunc: psr => psr
                    .Include(ps => ps.PodcastSubscription)
                    .Include(sct => sct.SubscriptionCycleType)
                )
                .Where(psr => psr.AccountId == accountId && psr.CancelledAt == null && psr.PodcastSubscription.PodcastShowId != null)
                .ToListAsync();
            List<PodcastSubscriptionRegistrationListItemResponseDTO> result = new();
            foreach (var psr in podcastSubscriptionsRegistrations)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(psr.AccountId.Value);
                result.Add(new PodcastSubscriptionRegistrationListItemResponseDTO
                {
                    Id = psr.Id,
                    Account = new AccountSnippetResponseDTO
                    {
                        Id = account.Id,
                        Email = account.Email,
                        FullName = account.FullName,
                        MainImageFileKey = account.MainImageFileKey
                    },
                    PodcastSubscriptionId = psr.PodcastSubscriptionId,
                    SubscriptionCycleType = psr.SubscriptionCycleType == null
                    ? null
                    : new SubscriptionCycleTypeDTO
                    {
                        Id = psr.SubscriptionCycleType.Id,
                        Name = psr.SubscriptionCycleType.Name
                    },
                    CurrentVersion = psr.CurrentVersion,
                    IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                    IsIncomeTaken = psr.IsIncomeTaken,
                    LastPaidAt = psr.LastPaidAt,
                    CancelledAt = psr.CancelledAt,
                    CreatedAt = psr.CreatedAt,
                    UpdatedAt = psr.UpdatedAt
                });
            };

            if (result == null || result.Count == 0)
            {
                _logger.LogWarning("No Podcast subscription registrations found for Account ID {AccountId}.", accountId);
                return null;
            }
            return result;
        }
        public async Task<PodcastSubscriptionRegistrationDetailResponseDTO> GetPodcastSubscriptionRegistrationByIdAsync(Guid PodcastSubscriptionRegistrationId)
        {
            var podcastSubscriptionRegistration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                .Include(psr => psr.PodcastSubscription)
                .ThenInclude(ps => ps.PodcastSubscriptionBenefitMappings)
                .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                .Where(psr => psr.Id == PodcastSubscriptionRegistrationId)
                .Select(psr => new PodcastSubscriptionRegistrationDetailResponseDTO
                {
                    Id = psr.Id,
                    AccountId = psr.AccountId ?? 0,
                    PodcastSubscriptionId = psr.PodcastSubscriptionId,
                    SubscriptionCycleType = psr.SubscriptionCycleType == null
                    ? null
                    : new SubscriptionCycleTypeDTO
                    {
                        Id = psr.SubscriptionCycleType.Id,
                        Name = psr.SubscriptionCycleType.Name
                    },
                    CurrentVersion = psr.CurrentVersion,
                    IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                    IsIncomeTaken = psr.IsIncomeTaken,
                    LastPaidAt = psr.LastPaidAt,
                    CancelledAt = psr.CancelledAt,
                    CreatedAt = psr.CreatedAt,
                    UpdatedAt = psr.UpdatedAt,
                    PodcastSubscriptionBenefitList = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
                        .Where(bm => bm.Version == psr.CurrentVersion)
                        .Select(bm => new PodcastSubscriptionBenefitDTO
                        {
                            Id = bm.PodcastSubscriptionBenefit.Id,
                            Name = bm.PodcastSubscriptionBenefit.Name
                        }).ToList()
                })
                .FirstOrDefaultAsync();
            if (podcastSubscriptionRegistration == null)
            {
                _logger.LogWarning("No Podcast subscription registration found with ID {PodcastSubscriptionRegistrationId}.", PodcastSubscriptionRegistrationId);
                return null;
            }
            return podcastSubscriptionRegistration;
        }
        public async Task ActivatePodcastSubscriptionAsync(ActivatePodcastSubscriptionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.IsActive == false && ps.DeletedAt == null);
                    if (podcastSubscription == null)
                    {
                        throw new Exception($"No Inactive Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }
                    PodcastShowDTO? show = null;
                    PodcastChannelDTO? channel = null;
                    if (podcastSubscription.PodcastShowId != null)
                    {
                        show = await GetPodcastShowWithAccountId(parameter.AccountId, podcastSubscription.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to activate PodcastSubscription for PodcastShow with Id: {podcastSubscription.PodcastShowId}");
                        }
                        //var showValidation = await ValidateShow(podcastSubscription.PodcastShowId.Value);
                        //if (!showValidation.isValid)
                        //{
                        //    throw new Exception(showValidation.errorMessage);
                        //}
                    }
                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, podcastSubscription.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to activate PodcastSubscription for PodcastChannel with Id: {podcastSubscription.PodcastChannelId}");
                        }
                        //var channelValidation = await ValidateChannel(podcastSubscription.PodcastChannelId.Value);
                        //if (!channelValidation.isValid)
                        //{
                        //    throw new Exception(channelValidation.errorMessage);
                        //}
                    }

                    if (show != null && show.PodcastChannelId != null)
                    {
                        var result = await _podcastSubscriptionGenericRepository.FindAll().
                            Where(ps => ps.PodcastChannelId.Equals(show.PodcastChannelId) && ps.IsActive == true && ps.DeletedAt == null)
                            .FirstOrDefaultAsync();
                        if (result != null)
                        {
                            throw new Exception($"An Active Channel Podcast Subscription exists for PodcastShow Id: {podcastSubscription.PodcastShowId}");
                        }
                    }
                    var existingActivePodcastSubscriptions = new List<PodcastSubscription>();
                    if(podcastSubscription.PodcastShowId != null)
                    {
                        existingActivePodcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll()
                            .Where(ps => ps.IsActive && ps.PodcastShowId == podcastSubscription.PodcastShowId)
                            .ToListAsync();
                    } else if(podcastSubscription.PodcastChannelId != null)
                    {
                        existingActivePodcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll()
                            .Where(ps => ps.IsActive && ps.PodcastChannelId == podcastSubscription.PodcastChannelId)
                            .ToListAsync();
                    }

                        foreach (var existingSubscription in existingActivePodcastSubscriptions)
                        {
                            existingSubscription.IsActive = false;
                            existingSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastSubscriptionGenericRepository.UpdateAsync(existingSubscription.Id, existingSubscription);

                            var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Where(sr => sr.PodcastSubscriptionId == existingSubscription.Id && sr.CancelledAt == null)
                                .ToListAsync();
                            foreach (var registration in subscriptionRegistrations)
                            {
                                var account = await _accountCachingService.GetAccountStatusCacheById(registration.AccountId.Value);
                                decimal refundAmount = 0;
                                if (!registration.IsIncomeTaken)
                                {
                                    var amount = existingSubscription.PodcastSubscriptionCycleTypePrices
                                        .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                        .Select(ptcp => ptcp.Price)
                                        .FirstOrDefault();
                                    refundAmount = amount;
                                    var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                                    var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                                        requestData: tempRequestData,
                                        sagaInstanceId: null,
                                        messageName: "podcast-subscription-refund-flow");
                                    await _messagingService.SendSagaMessageAsync(refundMessage, null);
                                }

                                var inactiveMailSendingRequestData = JObject.FromObject(new
                                {
                                    SendSubscriptionServiceEmailInfo = new
                                    {
                                        MailTypeName = "PodcastSubscriptionInactive",
                                        ToEmail = account.Email,
                                        MailObject = new PodcastSubscriptionInactiveMailViewModel
                                        {
                                            RefundAmount = refundAmount,
                                            CustomerFullName = account.FullName,
                                            CancelledDate = _dateHelper.GetNowByAppTimeZone(),
                                        }
                                    }
                                });
                                var customerMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                    requestData: inactiveMailSendingRequestData,
                                    sagaInstanceId: null,
                                    messageName: "subscription-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(customerMailSendingFlow);

                                await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                            }
                        }

                    podcastSubscription.IsActive = true;
                    podcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _podcastSubscriptionGenericRepository.UpdateAsync(podcastSubscription.Id, podcastSubscription);

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "AccountId", parameter.AccountId },
                        { "PodcastSubscriptionId", podcastSubscription.Id },
                        { "IsActive", podcastSubscription.IsActive },
                        { "UpdatedAt", podcastSubscription.UpdatedAt }
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully activate podcast subscription for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while activating podcast subscription for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Activate podcast subscription failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Activate podcast subscription failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task DeactivatePodcastSubscriptionAsync(DeactivatePodcastSubscriptionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.IsActive == true && ps.DeletedAt == null);
                    if (podcastSubscription == null)
                    {
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }
                    if (podcastSubscription.PodcastShowId != null)
                    {
                        var isValid = await GetPodcastShowWithAccountId(parameter.AccountId, podcastSubscription.PodcastShowId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to deactivate PodcastSubscription for PodcastShow with Id: {podcastSubscription.PodcastShowId}");
                        }
                        //var showValidation = await ValidateShow(podcastSubscription.PodcastShowId.Value);
                        //if (!showValidation.isValid)
                        //{
                        //    throw new Exception(showValidation.errorMessage);
                        //}
                    }
                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        var isValid = await GetPodcastChannelWithAccountId(parameter.AccountId, podcastSubscription.PodcastChannelId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to deactivate PodcastSubscription for PodcastChannel with Id: {podcastSubscription.PodcastChannelId}");
                        }
                        //var channelValidation = await ValidateChannel(podcastSubscription.PodcastChannelId.Value);
                        //if (!channelValidation.isValid)
                        //{
                        //    throw new Exception(channelValidation.errorMessage);
                        //}
                    }
                    podcastSubscription.IsActive = false;
                    podcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _podcastSubscriptionGenericRepository.UpdateAsync(podcastSubscription.Id, podcastSubscription);

                    var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                        .ToListAsync();
                    foreach (var registration in subscriptionRegistrations)
                    {
                        var account = await _accountCachingService.GetAccountStatusCacheById(registration.AccountId.Value);
                        decimal refundAmount = 0;
                        if (!registration.IsIncomeTaken)
                        {
                            var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                .Select(ptcp => ptcp.Price)
                                .FirstOrDefault();
                            refundAmount = amount;
                            var tempRequestData = new JObject
                            {
                                { "PodcastSubscriptionRegistrationId", registration.Id },
                                { "Profit", null },
                                { "AccountId", registration.AccountId },
                                { "Amount", amount },
                                { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                            };
                            var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: tempRequestData,
                                sagaInstanceId: null,
                                messageName: "podcast-subscription-refund-flow");
                            await _messagingService.SendSagaMessageAsync(refundMessage, null);
                        }

                        var inactiveMailSendingRequestData = JObject.FromObject(new
                        {
                            SendSubscriptionServiceEmailInfo = new
                            {
                                MailTypeName = "PodcastSubscriptionInactive",
                                ToEmail = account.Email,
                                MailObject = new PodcastSubscriptionInactiveMailViewModel
                                {
                                    RefundAmount = refundAmount,
                                    CustomerFullName = account.FullName,
                                    CancelledDate = _dateHelper.GetNowByAppTimeZone(),
                                }
                            }
                        });
                        var customerMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.SubscriptionManagementDomain,
                            requestData: inactiveMailSendingRequestData,
                            sagaInstanceId: null,
                            messageName: "subscription-service-mail-sending-flow");
                        await _messagingService.SendSagaMessageAsync(customerMailSendingFlow);

                        await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "AccountId", parameter.AccountId },
                        { "PodcastSubscriptionId", podcastSubscription.Id },
                        { "IsActive", podcastSubscription.IsActive },
                        { "UpdatedAt", podcastSubscription.UpdatedAt }
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully deactivate podcast subscription for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while deactivating podcast subscription for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Deactivate podcast subscription failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Deactivate podcast subscription failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelPodcastSubscriptionRegistrationAsync(CancelPodcastSubscriptionRegistrationParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscriptionRegistration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionRegistrationId && ps.CancelledAt == null);
                    if (podcastSubscriptionRegistration == null)
                    {
                        throw new Exception($"No Active Podcast Subscription Registration exists for Id: {parameter.PodcastSubscriptionRegistrationId}");
                    }
                    if (podcastSubscriptionRegistration.CancelledAt != null)
                    {
                        throw new Exception($"Podcast Subscription Registration has already been cancelled for Id: {parameter.PodcastSubscriptionRegistrationId}");
                    }
                    if (podcastSubscriptionRegistration.AccountId != parameter.AccountId)
                    {
                        throw new Exception($"The Logged In Account is unauthorized to cancel Podcast Subscription Registration with Id: {parameter.PodcastSubscriptionRegistrationId}");
                    }

                    var account = await _accountCachingService.GetAccountStatusCacheById(parameter.AccountId);

                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Where(ps => ps.Id == podcastSubscriptionRegistration.PodcastSubscriptionId && ps.DeletedAt == null)
                        .FirstOrDefaultAsync();

                    var systemConfig = await GetActiveSystemConfigProfile();
                    var profitRate = systemConfig.PodcastSubscriptionConfigs
                        .Where(psc => psc.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                        .Select(psc => psc.ProfitRate)
                        .FirstOrDefault();
                    var incomeTakenDelayDays = systemConfig.PodcastSubscriptionConfigs
                        .Where(psc => psc.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                        .Select(psc => psc.IncomeTakenDelayDays)
                        .FirstOrDefault();

                    decimal refundAmount = 0;
                    if (podcastSubscriptionRegistration.LastPaidAt.AddDays((double)incomeTakenDelayDays) < _dateHelper.GetNowByAppTimeZone())
                    {
                        podcastSubscriptionRegistration.IsIncomeTaken = true;
                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(podcastSubscriptionRegistration.Id, podcastSubscriptionRegistration);

                        var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                            .Select(ptcp => ptcp.Price)
                            .FirstOrDefault();

                        var amount = originalPrice - originalPrice * (decimal)profitRate;
                        var profit = originalPrice * (decimal)profitRate;

                        var transactionRequestData = new JObject
                        {
                            { "PodcastSubscriptionRegistrationId", podcastSubscriptionRegistration.Id },
                            { "Profit", profit },
                            { "AccountId", podcastSubscriptionRegistration.AccountId },
                            { "Amount", amount },
                            { "TransactionTypeId", (int)TransactionTypeEnum.PodcasterSubscriptionIncome }
                        };
                        var transactionMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.PaymentProcessingDomain,
                            requestData: transactionRequestData,
                            sagaInstanceId: null,
                            messageName: "podcaster-subscription-income-release-flow");
                        await _messagingService.SendSagaMessageAsync(transactionMessage, null);
                    }
                    else
                    {
                        var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                            .Select(ptcp => ptcp.Price)
                            .FirstOrDefault();
                        refundAmount = originalPrice;
                        var transactionRequestData = new JObject
                        {
                            { "PodcastSubscriptionRegistrationId", podcastSubscriptionRegistration.Id },
                            { "Profit", null },
                            { "AccountId", podcastSubscriptionRegistration.AccountId },
                            { "Amount", originalPrice },
                            { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                        };
                        var transactionMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.PaymentProcessingDomain,
                            requestData: transactionRequestData,
                            sagaInstanceId: null,
                            messageName: "podcast-subscription-refund-flow");
                        await _messagingService.SendSagaMessageAsync(transactionMessage, null);
                    }

                    podcastSubscriptionRegistration.CancelledAt = _dateHelper.GetNowByAppTimeZone();
                    var registrationResult = await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(podcastSubscriptionRegistration.Id, podcastSubscriptionRegistration);

                    var cancelRegistrationMailSendingRequestData = JObject.FromObject(new
                    {
                        SendSubscriptionServiceEmailInfo = new
                        {
                            MailTypeName = "PodcastSubscriptionRegistrationCancel",
                            ToEmail = account.Email,
                            MailObject = new PodcastSubscriptionRegistrationCancelMailViewModel
                            {
                                RefundAmount = refundAmount,
                                CustomerFullName = account.FullName,
                                CancelledDate = registrationResult.CancelledAt ?? _dateHelper.GetNowByAppTimeZone(),
                            }
                        }
                    });
                    var customerMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: cancelRegistrationMailSendingRequestData,
                        sagaInstanceId: null,
                        messageName: "subscription-service-mail-sending-flow");
                    await _messagingService.SendSagaMessageAsync(customerMailSendingFlow);

                    await transaction.CommitAsync();

                    var newResponseData = new JObject
                    {
                        { "AccountId", registrationResult.AccountId },
                        { "PodcastSubscriptionRegistrationId", registrationResult.Id },
                        { "CancelledAt", registrationResult.CancelledAt }
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully cancel podcast subscription registration for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while cancelling podcast subscription registration for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel podcast subscription registration failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel podcast subscription registration failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelPodcastSubscriptionAsync(CancelPodcastSubscriptionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Include(ps => ps.PodcastSubscriptionRegistrations)
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId);
                    if (podcastSubscription.DeletedAt != null)
                    {
                        throw new Exception($"Podcast Subscription has already been deleted for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }
                    if (parameter.IsActive != null)
                    {
                        if (podcastSubscription.IsActive == false)
                        {
                            throw new Exception($"Podcast Subscription is already inactive for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                        }
                        podcastSubscription.IsActive = false;
                        podcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastSubscriptionGenericRepository.UpdateAsync(podcastSubscription.Id, podcastSubscription);
                    }
                    if (parameter.IsDeleted != null)
                    {
                        podcastSubscription.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                        podcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastSubscriptionGenericRepository.UpdateAsync(podcastSubscription.Id, podcastSubscription);
                    }

                    var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                            .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                            .ToListAsync();
                    foreach (var registration in subscriptionRegistrations)
                    {
                        var account = await _accountCachingService.GetAccountStatusCacheById(registration.AccountId.Value);
                        decimal refundAmount = 0;
                        if (!registration.IsIncomeTaken)
                        {
                            var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                .Select(ptcp => ptcp.Price)
                                .FirstOrDefault();
                            refundAmount = amount;
                            var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                            var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: tempRequestData,
                                sagaInstanceId: null,
                                messageName: "podcast-subscription-refund-flow");
                            await _messagingService.SendSagaMessageAsync(refundMessage, null);
                        }

                        var cancelMailSendingRequestData = JObject.FromObject(new
                        {
                            SendSubscriptionServiceEmailInfo = new
                            {
                                MailTypeName = "PodcastSubscriptionCancel",
                                ToEmail = account.Email,
                                MailObject = new PodcastSubscriptionCancelMailViewModel
                                {
                                    RefundAmount = refundAmount,
                                    CustomerFullName = account.FullName,
                                    CancelledDate = _dateHelper.GetNowByAppTimeZone(),
                                }
                            }
                        });
                        var customerMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                            topic: KafkaTopicEnum.SubscriptionManagementDomain,
                            requestData: cancelMailSendingRequestData,
                            sagaInstanceId: null,
                            messageName: "subscription-service-mail-sending-flow");
                        await _messagingService.SendSagaMessageAsync(customerMailSendingFlow);

                        await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject()
                    {
                        { "PodcastSubscriptionId", podcastSubscription.Id },
                        { "IsActive", podcastSubscription.IsActive },
                        { "IsDeleted", parameter.IsDeleted }
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Cancel podcast subscription successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while cancelling podcast subscription for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel podcast subscription failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel podcast subscription failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        //public async Task PodcastSubscriptionRegistrationRenewalByHourAsync()
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var registrationsList = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
        //                .Where(psr => psr.CancelledAt == null
        //                    && psr.SubscriptionCycleTypeId == (int)SubscriptionTypeCycleEnum.Monthly
        //                    && psr.LastPaidAt.AddDays(30) < _dateHelper.GetNowByAppTimeZone() ||
        //                    psr.CancelledAt == null
        //                    && psr.SubscriptionCycleTypeId == (int)SubscriptionTypeCycleEnum.Annually
        //                    && psr.LastPaidAt.AddYears(1) < _dateHelper.GetNowByAppTimeZone())
        //                .ToListAsync();
        //            foreach (var registration in registrationsList)
        //            {
        //                if (registration.IsAcceptNewestVersionSwitch == null)
        //                {
        //                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
        //                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
        //                        .Where(ps => ps.Id == registration.PodcastSubscriptionId && ps.DeletedAt == null && ps.IsActive)
        //                        .FirstOrDefaultAsync();
        //                    registration.LastPaidAt = _dateHelper.GetNowByAppTimeZone();
        //                    var renewalRequestData = new JObject
        //                    {
        //                        { "PodcastSubscriptionRegistrationId", registration.Id },
        //                        { "Profit", null },
        //                        { "AccountId", registration.AccountId },
        //                        { "PodcasterId", null },
        //                        { "Amount", podcastSubscription.PodcastSubscriptionCycleTypePrices
        //                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId
        //                            && ptcp.Version == registration.CurrentVersion)
        //                            .Select(ptcp => ptcp.Price)
        //                            .FirstOrDefault() },
        //                        { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePayment }
        //                    };
        //                    var renewalMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.PaymentProcessingDomain,
        //                        requestData: renewalRequestData,
        //                        sagaInstanceId: null,
        //                        messageName: "podcast-subscription-payment-flow");
        //                    await _messagingService.SendSagaMessageAsync(renewalMessage, null);
        //                }
        //                if (registration.IsAcceptNewestVersionSwitch == true)
        //                {
        //                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
        //                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
        //                        .Where(ps => ps.Id == registration.PodcastSubscriptionId && ps.DeletedAt == null && ps.IsActive)
        //                        .FirstOrDefaultAsync();
        //                    registration.CurrentVersion = podcastSubscription.CurrentVersion;
        //                    registration.LastPaidAt = _dateHelper.GetNowByAppTimeZone();
        //                    var renewalRequestData = new JObject
        //                    {
        //                        { "PodcastSubscriptionRegistrationId", registration.Id },
        //                        { "Profit", null },
        //                        { "AccountId", registration.AccountId },
        //                        { "PodcasterId", null },
        //                        { "Amount", podcastSubscription.PodcastSubscriptionCycleTypePrices
        //                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId
        //                            && ptcp.Version == podcastSubscription.CurrentVersion)
        //                            .Select(ptcp => ptcp.Price)
        //                            .FirstOrDefault() },
        //                        { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePayment }
        //                    };
        //                    var renewalMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.PaymentProcessingDomain,
        //                        requestData: renewalRequestData,
        //                        sagaInstanceId: null,
        //                        messageName: "podcast-subscription-payment-flow");
        //                    await _messagingService.SendSagaMessageAsync(renewalMessage, null);
        //                }
        //                if (registration.IsAcceptNewestVersionSwitch == false)
        //                {
        //                    registration.CancelledAt = _dateHelper.GetNowByAppTimeZone();
        //                }
        //            }

        //            await transaction.CommitAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while checking podcast subscription registration renewal");
        //        }
        //    }
        //}
        //public async Task PodcastSubscriptionIncomeCheckingByHourAsync()
        //{
        //    using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var systemConfig = await GetActiveSystemConfigProfile();
        //            var podcastSubscriptionConfig = systemConfig.PodcastSubscriptionConfigs.ToList();

        //            var registrationsList = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
        //                .Where(psr => !psr.IsIncomeTaken
        //                    && psr.IsAcceptNewestVersionSwitch == null
        //                    && psr.CancelledAt != null
        //                    && psr.LastPaidAt.AddDays(30) > _dateHelper.GetNowByAppTimeZone())
        //                .ToListAsync();

        //            foreach (var registration in registrationsList)
        //            {
        //                // Find the config object for the current SubscriptionCycleTypeId
        //                var config = podcastSubscriptionConfig
        //                    .FirstOrDefault(psc => psc.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId);

        //                if (config != null)
        //                {
        //                    var podcasterId = 0;
        //                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
        //                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
        //                        .Where(ps => ps.Id == registration.PodcastSubscriptionId)
        //                        .FirstOrDefaultAsync();
        //                    if (podcastSubscription.PodcastChannelId != null)
        //                    {
        //                        var temp = await GetPodcastChannel(podcastSubscription.PodcastChannelId.Value);
        //                        podcasterId = temp.PodcasterId;
        //                    }
        //                    if (podcastSubscription.PodcastShowId != null)
        //                    {
        //                        var temp = await GetPodcastShow(podcastSubscription.PodcastShowId.Value);
        //                        podcasterId = temp.PodcasterId;
        //                    }
        //                    int incomeTakenDelayDays = config.IncomeTakenDelayDays;
        //                    decimal profitRate = (decimal)config.ProfitRate;
        //                    var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
        //                                .Where(psct => psct.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
        //                                .Select(psct => psct.Price)
        //                                .FirstOrDefault();
        //                    if (registration.LastPaidAt.AddDays(incomeTakenDelayDays) < _dateHelper.GetNowByAppTimeZone())
        //                    {
        //                        var incomeRequestData = new JObject()
        //                        {
        //                            { "PodcastSubscriptionRegistrationId", registration.Id },
        //                            { "Profit", originalPrice * profitRate},
        //                            { "AccountId", null },
        //                            { "PodcasterId", podcasterId },
        //                            { "Amount", originalPrice - originalPrice * profitRate },
        //                            { "TransactionTypeId", (int)TransactionTypeEnum.PodcasterSubscriptionIncome }
        //                        };
        //                        var incomeMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
        //                        topic: KafkaTopicEnum.PaymentProcessingDomain,
        //                        requestData: incomeRequestData,
        //                        sagaInstanceId: null,
        //                        messageName: "podcaster-subscription-income-release-flow");
        //                        await _messagingService.SendSagaMessageAsync(incomeMessage, null);
        //                        registration.IsIncomeTaken = true;
        //                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);
        //                    }
        //                }
        //            }

        //            await transaction.CommitAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error occurred while checking podcast subscription income");
        //        }
        //    }
        //}
        public async Task CancelShowSubscriptionDmcaRemoveShowForceAsync(CancelShowSubscriptionDmcaRemoveShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Include(ps => ps.PodcastSubscriptionRegistrations))
                        .Where(ps => ps.PodcastShowId == parameter.PodcastShowId
                        && ps.IsActive)
                        .ToListAsync();

                    foreach (var podcastSubscription in podcastSubscriptions)
                    {
                        var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                                .ToListAsync();
                        foreach (var registration in subscriptionRegistrations)
                        {
                            if (!registration.IsIncomeTaken)
                            {
                                var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault();
                                var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                                var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: tempRequestData,
                                    sagaInstanceId: null,
                                    messageName: "podcast-subscription-refund-flow");
                                await _messagingService.SendSagaMessageAsync(refundMessage, null);
                            }
                            await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Cancel Show Subscription Dmca Remove Show Force for Saga Id: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Show Subscription Dmca Remove Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel Show Subscription Dmca Remove Show Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel Show Subscription Dmca Remove Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelShowSubscriptionUnpublishShowForceAsync(CancelShowSubscriptionUnpublishShowForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Include(ps => ps.PodcastSubscriptionRegistrations))
                        .Where(ps => ps.PodcastShowId == parameter.PodcastShowId
                        && ps.IsActive)
                        .ToListAsync();

                    foreach (var podcastSubscription in podcastSubscriptions)
                    {
                        var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                                .ToListAsync();
                        foreach (var registration in subscriptionRegistrations)
                        {
                            if (!registration.IsIncomeTaken)
                            {
                                var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault();
                                var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                                var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: tempRequestData,
                                    sagaInstanceId: null,
                                    messageName: "podcast-subscription-refund-flow");
                                await _messagingService.SendSagaMessageAsync(refundMessage, null);
                            }
                            await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Cancel Show Subscription Unpublish Show Force for Saga Id: {SagaId}", command.SagaInstanceId);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Show Subscription Unpublish Show Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel Show Subscription Unpublish Show Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel Show Subscription Unpublish Show Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelChannelSubscriptionUnpublishChannelForceAsync(CancelChannelSubscriptionUnpublishChannelForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Include(ps => ps.PodcastSubscriptionRegistrations))
                        .Where(ps => ps.PodcastChannelId == parameter.PodcastChannelId
                        && ps.IsActive)
                        .ToListAsync();

                    foreach (var podcastSubscription in podcastSubscriptions)
                    {
                        var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                                .ToListAsync();
                        foreach (var registration in subscriptionRegistrations)
                        {
                            if (!registration.IsIncomeTaken)
                            {
                                var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault();
                                var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                                var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: tempRequestData,
                                    sagaInstanceId: null,
                                    messageName: "podcast-subscription-refund-flow");
                                await _messagingService.SendSagaMessageAsync(refundMessage, null);
                            }
                            await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Cancel Channel Subscription Unpublish Channel Force for Saga Id: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Channel Subscription Unpublish Channel Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel Channel Subscription Unpublish Channel Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel Channel Subscription Unpublish Channel Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelShowSubscriptionShowDeletionForceAsync(CancelShowSubscriptionShowDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Include(ps => ps.PodcastSubscriptionRegistrations))
                        .Where(ps => ps.PodcastShowId == parameter.PodcastShowId
                        && ps.IsActive)
                        .ToListAsync();

                    foreach (var podcastSubscription in podcastSubscriptions)
                    {
                        var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                                .ToListAsync();
                        foreach (var registration in subscriptionRegistrations)
                        {
                            if (!registration.IsIncomeTaken)
                            {
                                var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault();
                                var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                                var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: tempRequestData,
                                    sagaInstanceId: null,
                                    messageName: "podcast-subscription-refund-flow");
                                await _messagingService.SendSagaMessageAsync(refundMessage, null);
                            }
                            await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Cancel Show Subscription Show Deletion Force for Saga Id: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Show Subscription Show Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel Show Subscription Show Deletion Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel Show Subscription Show Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelChannelSubscriptionChannelDeletionForceAsync(CancelChannelSubscriptionChannelDeletionForceParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Include(ps => ps.PodcastSubscriptionRegistrations))
                        .Where(ps => ps.PodcastChannelId == parameter.PodcastChannelId
                        && ps.IsActive)
                        .ToListAsync();

                    foreach (var podcastSubscription in podcastSubscriptions)
                    {
                        var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                                .ToListAsync();
                        foreach (var registration in subscriptionRegistrations)
                        {
                            if (!registration.IsIncomeTaken)
                            {
                                var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault();
                                var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                                var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.PaymentProcessingDomain,
                                    requestData: tempRequestData,
                                    sagaInstanceId: null,
                                    messageName: "podcast-subscription-refund-flow");
                                await _messagingService.SendSagaMessageAsync(refundMessage, null);
                            }
                            await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Cancel Channel Subscription Channel Deletion Force for Saga Id: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Channel Subscription Channel Deletion Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel Channel Subscription Channel Deletion Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel Channel Subscription Channel Deletion Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CancelPodcasterChannelsSubscriptionTerminatePodcasterForceAsync(CancelPodcasterChannelsSubscriptionTerminatePodcasterForceParameterDTO paremeter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var channelList = await GetPodcastChannelsWithAccountId(paremeter.PodcasterId);
                    foreach (var channel in channelList)
                    {
                        var podcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                        includeFunc: function => function
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Include(ps => ps.PodcastSubscriptionRegistrations))
                        .Where(ps => ps.PodcastChannelId == channel.Id
                        && ps.IsActive)
                        .ToListAsync();

                        foreach (var podcastSubscription in podcastSubscriptions)
                        {
                            var subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                    .Where(sr => sr.PodcastSubscriptionId == podcastSubscription.Id && sr.CancelledAt == null)
                                    .ToListAsync();
                            foreach (var registration in subscriptionRegistrations)
                            {
                                if (!registration.IsIncomeTaken)
                                {
                                    var amount = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                        .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                        .Select(ptcp => ptcp.Price)
                                        .FirstOrDefault();
                                    var tempRequestData = new JObject
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", null },
                                    { "AccountId", registration.AccountId },
                                    { "Amount", amount },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                };
                                    var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                                        requestData: tempRequestData,
                                        sagaInstanceId: null,
                                        messageName: "podcast-subscription-refund-flow");
                                    await _messagingService.SendSagaMessageAsync(refundMessage, null);
                                }
                                await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Cancel Podcaster Channels Subscription Terminate Podcaster Force for Saga Id: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Podcaster Channels Subscription Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel Podcaster Channels Subscription Terminate Podcaster Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel Podcaster Channels Subscription Terminate Podcaster Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task PodcastSubscriptionIncomeReleaseAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var systemConfig = await GetActiveSystemConfigProfile();
                    var podcastSubscriptionConfig = systemConfig.PodcastSubscriptionConfigs.ToList();

                    var registrationsList = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(psr => !psr.IsIncomeTaken
                            && psr.CancelledAt == null)
                        .ToListAsync();

                    //Console.WriteLine($"Found {registrationsList.Count} podcast subscription registrations for income release.");
                    //Console.WriteLine($"Registrations List: {JArray.FromObject(registrationsList)}");

                    foreach (var registration in registrationsList)
                    {
                        // Find the config object for the current SubscriptionCycleTypeId
                        var config = podcastSubscriptionConfig
                            .FirstOrDefault(psc => psc.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId);

                        if (config != null)
                        {
                            var podcasterId = 0;
                            var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                                .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                                .Where(ps => ps.Id == registration.PodcastSubscriptionId)
                                .FirstOrDefaultAsync();
                            if (podcastSubscription.PodcastChannelId != null)
                            {
                                var temp = await GetPodcastChannel(podcastSubscription.PodcastChannelId.Value);
                                podcasterId = temp.PodcasterId;
                            }
                            if (podcastSubscription.PodcastShowId != null)
                            {
                                var temp = await GetPodcastShow(podcastSubscription.PodcastShowId.Value);
                                podcasterId = temp.PodcasterId;
                            }
                            //int incomeTakenDelayDays = config.IncomeTakenDelayDays;
                            int incomeTakenDelayDays = 0;
                            decimal profitRate = (decimal)config.ProfitRate;
                            var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                        .Where(psct => psct.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                        .Select(psct => psct.Price)
                                        .FirstOrDefault();
                            if (registration.LastPaidAt.AddDays(incomeTakenDelayDays) < _dateHelper.GetNowByAppTimeZone())
                            {
                                var incomeRequestData = new JObject()
                                {
                                    { "PodcastSubscriptionRegistrationId", registration.Id },
                                    { "Profit", originalPrice * profitRate},
                                    { "AccountId", null },
                                    { "PodcasterId", podcasterId },
                                    { "Amount", originalPrice - originalPrice * profitRate },
                                    { "TransactionTypeId", (int)TransactionTypeEnum.PodcasterSubscriptionIncome }
                                };
                                var incomeMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: incomeRequestData,
                                sagaInstanceId: null,
                                messageName: "podcaster-subscription-income-release-flow");
                                await _messagingService.SendSagaMessageAsync(incomeMessage, null);
                                registration.IsIncomeTaken = true;
                                await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);
                            }
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while checking podcast subscription income");
                }
            }
        }
        public async Task PodcastSubscriptionRegistrationRenewalAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var registrationsList = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(psr => psr.CancelledAt == null
                            && psr.SubscriptionCycleTypeId == (int)SubscriptionTypeCycleEnum.Monthly
                            && psr.LastPaidAt.AddDays(30) < _dateHelper.GetNowByAppTimeZone() ||
                            psr.CancelledAt == null
                            && psr.SubscriptionCycleTypeId == (int)SubscriptionTypeCycleEnum.Annually
                            && psr.LastPaidAt.AddYears(1) < _dateHelper.GetNowByAppTimeZone())
                        .ToListAsync();
                    foreach (var registration in registrationsList)
                    {
                        var account = await GetAccount(registration.AccountId.Value);
                        if (registration.IsAcceptNewestVersionSwitch == null)
                        {
                            var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                                .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                                .Where(ps => ps.Id == registration.PodcastSubscriptionId && ps.DeletedAt == null && ps.IsActive)
                                .FirstOrDefaultAsync();
                            var price = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId
                                    && ptcp.Version == registration.CurrentVersion)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault();

                            if (account.Balance - price <= 0)
                            {
                                registration.CancelledAt = _dateHelper.GetNowByAppTimeZone();
                                await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);

                                var registrationRenewalFailureMailSendingRequestData = JObject.FromObject(new
                                {
                                    SendSubscriptionServiceEmailInfo = new
                                    {
                                        MailTypeName = "PodcastSubscriptionRegistrationRenewalFailure",
                                        ToEmail = account.Email,
                                        MailObject = new PodcastSubscriptionRegistrationRenewalFailureMailViewModel
                                        {
                                            CustomerFullName = account.FullName,
                                            Price = price,
                                            UpdatedDate = _dateHelper.GetNowByAppTimeZone(),
                                        }
                                    }
                                });
                                var customerMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                    requestData: registrationRenewalFailureMailSendingRequestData,
                                    sagaInstanceId: null,
                                    messageName: "subscription-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(customerMailSendingFlow1);

                                continue;
                            }
                            
                            registration.LastPaidAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);

                            var registrationRenewalSuccessMailSendingRequestData = JObject.FromObject(new
                            {
                                SendSubscriptionServiceEmailInfo = new
                                {
                                    MailTypeName = "PodcastSubscriptionRegistrationRenewalSuccess",
                                    ToEmail = account.Email,
                                    MailObject = new PodcastSubscriptionRegistrationRenewalSuccessMailViewModel
                                    {
                                        CustomerFullName = account.FullName,
                                        Price = price,
                                        UpdatedDate = _dateHelper.GetNowByAppTimeZone(),
                                    }
                                }
                            });
                            var customerMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                requestData: registrationRenewalSuccessMailSendingRequestData,
                                sagaInstanceId: null,
                                messageName: "subscription-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(customerMailSendingFlow2);

                            var renewalRequestData = new JObject
                            {
                                { "PodcastSubscriptionRegistrationId", registration.Id },
                                { "Profit", null },
                                { "AccountId", registration.AccountId },
                                { "PodcasterId", null },
                                { "Amount", price},
                                { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePayment }
                            };
                            var renewalMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: renewalRequestData,
                                sagaInstanceId: null,
                                messageName: "podcast-subscription-payment-flow");
                            await _messagingService.SendSagaMessageAsync(renewalMessage, null);
                        }
                        if (registration.IsAcceptNewestVersionSwitch == true)
                        {
                            var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                                .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                                .Where(ps => ps.Id == registration.PodcastSubscriptionId && ps.DeletedAt == null && ps.IsActive)
                                .FirstOrDefaultAsync();
                            var price = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId
                                    && ptcp.Version == registration.CurrentVersion)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault();

                            if (account.Balance - price <= 0)
                            {
                                registration.CancelledAt = _dateHelper.GetNowByAppTimeZone();
                                await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);

                                var registrationRenewalFailureMailSendingRequestData = JObject.FromObject(new
                                {
                                    SendSubscriptionServiceEmailInfo = new
                                    {
                                        MailTypeName = "PodcastSubscriptionRegistrationRenewalFailure",
                                        ToEmail = account.Email,
                                        MailObject = new PodcastSubscriptionRegistrationRenewalFailureMailViewModel
                                        {
                                            CustomerFullName = account.FullName,
                                            Price = price,
                                            UpdatedDate = _dateHelper.GetNowByAppTimeZone(),
                                        }
                                    }
                                });
                                var customerMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                    topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                    requestData: registrationRenewalFailureMailSendingRequestData,
                                    sagaInstanceId: null,
                                    messageName: "subscription-service-mail-sending-flow");
                                await _messagingService.SendSagaMessageAsync(customerMailSendingFlow1);

                                continue;
                            }
                            registration.CurrentVersion = podcastSubscription.CurrentVersion;
                            registration.LastPaidAt = _dateHelper.GetNowByAppTimeZone();

                            await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);

                            var renewalRequestData = new JObject
                            {
                                { "PodcastSubscriptionRegistrationId", registration.Id },
                                { "Profit", null },
                                { "AccountId", registration.AccountId },
                                { "PodcasterId", null },
                                { "Amount", price },
                                { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePayment }
                            };
                            var renewalMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: renewalRequestData,
                                sagaInstanceId: null,
                                messageName: "podcast-subscription-payment-flow");
                            await _messagingService.SendSagaMessageAsync(renewalMessage, null);

                            var registrationRenewalSuccessMailSendingRequestData = JObject.FromObject(new
                            {
                                SendSubscriptionServiceEmailInfo = new
                                {
                                    MailTypeName = "PodcastSubscriptionRegistrationRenewalSuccess",
                                    ToEmail = account.Email,
                                    MailObject = new PodcastSubscriptionRegistrationRenewalSuccessMailViewModel
                                    {
                                        CustomerFullName = account.FullName,
                                        Price = price,
                                        UpdatedDate = _dateHelper.GetNowByAppTimeZone(),
                                    }
                                }
                            });
                            var customerMailSendingFlow2 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                requestData: registrationRenewalSuccessMailSendingRequestData,
                                sagaInstanceId: null,
                                messageName: "subscription-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(customerMailSendingFlow2);

                        }
                        if (registration.IsAcceptNewestVersionSwitch == false)
                        {
                            var registrationCancelMailSendingRequestData = JObject.FromObject(new
                            {
                                SendSubscriptionServiceEmailInfo = new
                                {
                                    MailTypeName = "PodcastSubscriptionRegistrationCancel",
                                    ToEmail = account.Email,
                                    MailObject = new PodcastSubscriptionRegistrationCancelMailViewModel
                                    {
                                        CustomerFullName = account.FullName,
                                        RefundAmount = null,
                                        CancelledDate = _dateHelper.GetNowByAppTimeZone(),
                                    }
                                }
                            });
                            var customerMailSendingFlow1 = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                requestData: registrationCancelMailSendingRequestData,
                                sagaInstanceId: null,
                                messageName: "subscription-service-mail-sending-flow");
                            await _messagingService.SendSagaMessageAsync(customerMailSendingFlow1);

                            registration.CancelledAt = _dateHelper.GetNowByAppTimeZone();
                            await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while checking podcast subscription registration renewal");
                }
            }
        }
        public async Task CancelPodcastSubscriptionRegistrationChannelShowsAsync(CancelPodcastSubscriptionRegistrationChannelShowsParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(parameter.PodcasterId);

                    if(podcaster == null)
                    {
                        throw new Exception("Podcaster account is not found");
                    }
                    if (!podcaster.HasVerifiedPodcasterProfile)
                    {
                        throw new Exception("Podcaster account is not verified");
                    }

                    if(parameter.PodcastChannelId != null)
                    {
                        var showPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                            .Where(ps => ps.PodcastShowId == parameter.PodcastShowId && ps.IsActive && ps.DeletedAt == null)
                            .FirstOrDefaultAsync();

                        var channelPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                            .Where(ps => ps.PodcastChannelId == parameter.PodcastChannelId && ps.IsActive && ps.DeletedAt == null)
                            .FirstOrDefaultAsync();

                        List<PodcastSubscriptionRegistration> subscriptionRegistrations = new List<PodcastSubscriptionRegistration>();
                        List<PodcastSubscriptionRegistration> subscriptionRegistrationsChannel = new List<PodcastSubscriptionRegistration>();

                        if(showPodcastSubscription != null)
                        {
                            subscriptionRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                            .Where(sr => sr.PodcastSubscriptionId == showPodcastSubscription.Id && sr.CancelledAt == null)
                            .ToListAsync();
                        }
                        
                        if(channelPodcastSubscription != null)
                        {
                            subscriptionRegistrationsChannel = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                            .Where(sr => sr.PodcastSubscriptionId == channelPodcastSubscription.Id && sr.CancelledAt == null)
                            .ToListAsync();
                        }

                        foreach (var registration in subscriptionRegistrations)
                        {
                            foreach (var channelRegistration in subscriptionRegistrationsChannel)
                            {
                                if(registration.AccountId == channelRegistration.AccountId)
                                {
                                    var account = await _accountCachingService.GetAccountStatusCacheById(registration.AccountId.Value);
                                    decimal refundAmount = 0;
                                    if (!registration.IsIncomeTaken)
                                    {
                                        var amount = showPodcastSubscription.PodcastSubscriptionCycleTypePrices
                                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId)
                                            .Select(ptcp => ptcp.Price)
                                            .FirstOrDefault();
                                        refundAmount = amount;
                                        var tempRequestData = new JObject
                                        {
                                            { "PodcastSubscriptionRegistrationId", registration.Id },
                                            { "Profit", null },
                                            { "AccountId", registration.AccountId },
                                            { "Amount", amount },
                                            { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund }
                                        };
                                        var refundMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                            topic: KafkaTopicEnum.PaymentProcessingDomain,
                                            requestData: tempRequestData,
                                            sagaInstanceId: null,
                                            messageName: "podcast-subscription-refund-flow");
                                        await _messagingService.SendSagaMessageAsync(refundMessage, null);
                                    }

                                    var duplicateMailSendingRequestData = JObject.FromObject(new
                                    {
                                        SendSubscriptionServiceEmailInfo = new
                                        {
                                            MailTypeName = "PodcastSubscriptionDuplicate",
                                            ToEmail = account.Email,
                                            MailObject = new PodcastSubscriptionDuplicateMailViewModel
                                            {
                                                RefundAmount = refundAmount,
                                                CustomerFullName = account.FullName,
                                                CancelledDate = _dateHelper.GetNowByAppTimeZone(),
                                            }
                                        }
                                    });
                                    var customerMailSendingFlow = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                                        requestData: duplicateMailSendingRequestData,
                                        sagaInstanceId: null,
                                        messageName: "subscription-service-mail-sending-flow");
                                    await _messagingService.SendSagaMessageAsync(customerMailSendingFlow);

                                    await _podcastSubscriptionRegistrationGenericRepository.DeleteAsync(registration.Id);
                                }
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Cancel Podcaster Channels Subscription Terminate Podcaster Force for Saga Id: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Cancel Podcaster Channels Subscription Terminate Podcaster Force for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Cancel Podcaster Channels Subscription Terminate Podcaster Force failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Cancel Podcaster Channels Subscription Terminate Podcaster Force failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task AcceptPodcastSubscriptionNewestVersionAsync(AcceptPodcastSubscriptionNewestVersionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    if(!await ValidatePodcastSubscriptionRegistration(parameter.AccountId, parameter.PodcastSubscriptionRegistrationId))
                    {
                        throw new Exception("Podcast Subscription Registration is not found for the account");
                    }

                    var registration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                            .Where(psr => psr.Id == parameter.PodcastSubscriptionRegistrationId)
                            .FirstOrDefaultAsync();
                    var subscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .Where(ps => ps.Id == registration.PodcastSubscriptionId)
                            .FirstOrDefaultAsync();
                    if(registration.CurrentVersion == subscription.CurrentVersion)
                    {
                        throw new Exception("Podcast Subscription Registration is already at the newest version");
                    }

                    if (parameter.IsAccepted)
                    {
                        registration.IsAcceptNewestVersionSwitch = true;
                        registration.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);
                    }
                    else
                    {
                        registration.IsAcceptNewestVersionSwitch = false;
                        registration.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);
                    }
                        await transaction.CommitAsync();

                    var newResponseData = command.RequestData;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Successfully Accept Podcast Subscription Newest Version for Saga Id: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while Accept Podcast Subscription Newest Version for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Accept Podcast Subscription Newest Version failed, error: " + ex.Message }
                    };
                    var newMessageName = command.MessageName + ".failed";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.SubscriptionManagementDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Accept Podcast Subscription Newest Version failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task<PodcastSubscriptionDetailResponseDTO> GetActivePodcastSubscriptionByPodcastChannelIdAsync(Guid podcastChannelId, int? accountId)
        {
            try
            {
                var result = await _podcastSubscriptionGenericRepository.FindAll(
                    includeFunc: ps => ps
                        .Include(sct => sct.PodcastSubscriptionCycleTypePrices)
                        .ThenInclude(sct => sct.SubscriptionCycleType)
                        .Include(bm => bm.PodcastSubscriptionBenefitMappings)
                        .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                        .Include(sr => sr.PodcastSubscriptionRegistrations)
                        .ThenInclude(sr => sr.SubscriptionCycleType)
                    )
                    .Where(ps => ps.PodcastChannelId == podcastChannelId && ps.IsActive && ps.DeletedAt == null)
                    .Select(ps => new PodcastSubscriptionDetailResponseDTO
                    {
                        Id = ps.Id,
                        Name = ps.Name,
                        Description = ps.Description,
                        PodcastShowId = ps.PodcastShowId,
                        PodcastChannelId = ps.PodcastChannelId,
                        IsActive = ps.IsActive,
                        CurrentVersion = ps.CurrentVersion,
                        DeletedAt = ps.DeletedAt,
                        CreatedAt = ps.CreatedAt,
                        UpdatedAt = ps.UpdatedAt,
                        PodcastSubscriptionCycleTypePriceList = ps.PodcastSubscriptionCycleTypePrices
                            .Where(ctp => ctp.Version == ps.CurrentVersion)
                            .Select(ctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                            {
                                PodcastSubscriptionId = ctp.PodcastSubscriptionId,
                                SubscriptionCycleType = ctp.SubscriptionCycleType == null
                                ? null
                                : new SubscriptionCycleTypeDTO
                                {
                                    Id = ctp.SubscriptionCycleType.Id,
                                    Name = ctp.SubscriptionCycleType.Name
                                },
                                Version = ctp.Version,
                                Price = ctp.Price,
                                CreatedAt = ctp.CreatedAt,
                                UpdatedAt = ctp.UpdatedAt
                            }).ToList(),
                        PodcastSubscriptionBenefitMappingList = ps.PodcastSubscriptionBenefitMappings
                            .Where(bm => bm.Version == ps.CurrentVersion)
                            .Select(bm => new DTOs.PodcastSubscription.ListItems.PodcastSubscriptionBenefitMappingListItemResponseDTO
                            {
                                PodcastSubscriptionId = bm.PodcastSubscriptionId,
                                PodcastSubscriptionBenefit = bm.PodcastSubscriptionBenefit == null
                                ? null
                                : new PodcastSubscriptionBenefitDTO
                                {
                                    Id = bm.PodcastSubscriptionBenefit.Id,
                                    Name = bm.PodcastSubscriptionBenefit.Name
                                },
                                Version = bm.Version,
                                CreatedAt = bm.CreatedAt,
                                UpdatedAt = bm.UpdatedAt
                            }).ToList(),
                        PodcastSubscriptionRegistrationList = null
                    })
                    .FirstOrDefaultAsync();
                //if (accountId != null)
                //{
                //    var temp = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                //        .Where(psr => psr.AccountId == accountId && psr.CancelledAt == null && psr.PodcastSubscriptionId == result.Id)
                //        .FirstOrDefaultAsync();
                //    if (temp != null)
                //        return null;
                //}
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetActivePodcastSubscriptionByPodcastChannelIdAsync for PodcastChannelId: {PodcastChannelId}", podcastChannelId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription for PodcastChannelId: {podcastChannelId}. Error: {ex.Message}");
            }
        }
        public async Task<PodcastSubscriptionDetailResponseDTO> GetActivePodcastSubscriptionByPodcastShowIdAsync(Guid podcastShowId, int? accountId)
        {
            try
            {
                var show = await GetPodcastShow(podcastShowId);
                if(show == null)
                {
                    throw new HttpRequestException("Podcast show not found");
                }
                PodcastSubscription? temp = null;
                if(show.PodcastChannelId != null)
                {
                    temp = await _podcastSubscriptionGenericRepository.FindAll()
                    .Where(ps => ps.PodcastChannelId == show.PodcastChannelId && ps.IsActive && ps.DeletedAt == null)
                    .FirstOrDefaultAsync();
                }

                var query = _podcastSubscriptionGenericRepository.FindAll(
                    includeFunc: ps => ps
                        .Include(sct => sct.PodcastSubscriptionCycleTypePrices)
                        .ThenInclude(sct => sct.SubscriptionCycleType)
                        .Include(bm => bm.PodcastSubscriptionBenefitMappings)
                        .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                        .Include(sr => sr.PodcastSubscriptionRegistrations)
                        .ThenInclude(sr => sr.SubscriptionCycleType)
                    );

                if (temp != null)
                {
                    query = query.Where(ps => ps.PodcastChannelId == show.PodcastChannelId);
                }
                else
                {
                    query = query.Where(ps => ps.PodcastShowId == podcastShowId);
                }
                var result = await query
                        .Where(ps => ps.IsActive && ps.DeletedAt == null)
                        .Select(ps => new PodcastSubscriptionDetailResponseDTO
                        {
                            Id = ps.Id,
                            Name = ps.Name,
                            Description = ps.Description,
                            PodcastShowId = ps.PodcastShowId,
                            PodcastChannelId = ps.PodcastChannelId,
                            IsActive = ps.IsActive,
                            CurrentVersion = ps.CurrentVersion,
                            DeletedAt = ps.DeletedAt,
                            CreatedAt = ps.CreatedAt,
                            UpdatedAt = ps.UpdatedAt,
                            PodcastSubscriptionCycleTypePriceList = ps.PodcastSubscriptionCycleTypePrices
                                .Where(ctp => ctp.Version == ps.CurrentVersion)
                                .Select(ctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                                {
                                    PodcastSubscriptionId = ctp.PodcastSubscriptionId,
                                    SubscriptionCycleType = ctp.SubscriptionCycleType == null
                                    ? null
                                    : new SubscriptionCycleTypeDTO
                                    {
                                        Id = ctp.SubscriptionCycleType.Id,
                                        Name = ctp.SubscriptionCycleType.Name
                                    },
                                    Version = ctp.Version,
                                    Price = ctp.Price,
                                    CreatedAt = ctp.CreatedAt,
                                    UpdatedAt = ctp.UpdatedAt
                                }).ToList(),
                            PodcastSubscriptionBenefitMappingList = ps.PodcastSubscriptionBenefitMappings
                                .Where(bm => bm.Version == ps.CurrentVersion)
                                .Select(bm => new DTOs.PodcastSubscription.ListItems.PodcastSubscriptionBenefitMappingListItemResponseDTO
                                {
                                    PodcastSubscriptionId = bm.PodcastSubscriptionId,
                                    PodcastSubscriptionBenefit = bm.PodcastSubscriptionBenefit == null
                                    ? null
                                    : new PodcastSubscriptionBenefitDTO
                                    {
                                        Id = bm.PodcastSubscriptionBenefit.Id,
                                        Name = bm.PodcastSubscriptionBenefit.Name
                                    },
                                    Version = bm.Version,
                                    CreatedAt = bm.CreatedAt,
                                    UpdatedAt = bm.UpdatedAt
                                }).ToList(),
                            PodcastSubscriptionRegistrationList = null
                        })
                        .FirstOrDefaultAsync();

                //if (accountId != null)
                //{
                //    var temp2 = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                //        .Where(psr => psr.AccountId == accountId && psr.CancelledAt == null && psr.PodcastSubscriptionId == result.Id)
                //        .FirstOrDefaultAsync();
                //    if (temp2 != null)
                //        return null;
                //}

                return result;
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetActivePodcastSubscriptionByPodcastShowIdAsync for PodcastShowId: {PodcastShowId}", podcastShowId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription for PodcastShowId: {podcastShowId}. Error: {ex.Message}");
            }
        }
        public async Task<PodcastSubscriptionRegistrationDetailResponseDTO> GetPodcastSubscriptionRegistrationsByPodcastEpisodeIdAsync(Guid podcastEpisodeId, int accountId)
        {
            try
            {
                PodcastSubscriptionRegistrationDetailResponseDTO? result = null;
                var episode = await GetPodcastEpisode(podcastEpisodeId);
                if (episode == null)
                {
                    throw new HttpRequestException("Podcast episode not found");
                }
                if (episode.PodcastShowId != null)
                {
                    result = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Include(psr => psr.PodcastSubscription)
                        .ThenInclude(ps => ps.PodcastSubscriptionBenefitMappings)
                        .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                        .Where(psr => psr.AccountId == accountId && psr.PodcastSubscription.PodcastShowId == episode.PodcastShowId && psr.CancelledAt == null)
                        .Select(psr => new PodcastSubscriptionRegistrationDetailResponseDTO
                        {
                            Id = psr.Id,
                            AccountId = psr.AccountId ?? 0,
                            PodcastSubscriptionId = psr.PodcastSubscriptionId,
                            SubscriptionCycleType = psr.SubscriptionCycleType == null
                            ? null
                            : new SubscriptionCycleTypeDTO
                            {
                                Id = psr.SubscriptionCycleType.Id,
                                Name = psr.SubscriptionCycleType.Name
                            },
                            CurrentVersion = psr.CurrentVersion,
                            IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                            IsIncomeTaken = psr.IsIncomeTaken,
                            LastPaidAt = psr.LastPaidAt,
                            CancelledAt = psr.CancelledAt,
                            CreatedAt = psr.CreatedAt,
                            UpdatedAt = psr.UpdatedAt,
                            PodcastSubscriptionBenefitList = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
                                .Where(psb => psb.Version == psr.CurrentVersion)
                                .Select(bm => new PodcastSubscriptionBenefitDTO
                                {
                                    Id = bm.PodcastSubscriptionBenefit.Id,
                                    Name = bm.PodcastSubscriptionBenefit.Name
                                }).ToList()
                        })
                        .FirstOrDefaultAsync();
                    if(result == null)
                    {
                        var show = await GetPodcastShow(episode.PodcastShowId);
                        if (show.PodcastChannelId != null)
                        {
                            result = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                                .Include(psr => psr.PodcastSubscription)
                                .ThenInclude(ps => ps.PodcastSubscriptionBenefitMappings)
                                .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                                .Where(psr => psr.AccountId == accountId && psr.PodcastSubscription.PodcastChannelId == show.PodcastChannelId && psr.CancelledAt == null)
                                .Select(psr => new PodcastSubscriptionRegistrationDetailResponseDTO
                                {
                                    Id = psr.Id,
                                    AccountId = psr.AccountId ?? 0,
                                    PodcastSubscriptionId = psr.PodcastSubscriptionId,
                                    SubscriptionCycleType = psr.SubscriptionCycleType == null
                                    ? null
                                    : new SubscriptionCycleTypeDTO
                                    {
                                        Id = psr.SubscriptionCycleType.Id,
                                        Name = psr.SubscriptionCycleType.Name
                                    },
                                    CurrentVersion = psr.CurrentVersion,
                                    IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                                    IsIncomeTaken = psr.IsIncomeTaken,
                                    LastPaidAt = psr.LastPaidAt,
                                    CancelledAt = psr.CancelledAt,
                                    CreatedAt = psr.CreatedAt,
                                    UpdatedAt = psr.UpdatedAt,
                                    PodcastSubscriptionBenefitList = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
                                        .Where(psb => psb.Version == psr.CurrentVersion)
                                        .Select(bm => new PodcastSubscriptionBenefitDTO
                                        {
                                            Id = bm.PodcastSubscriptionBenefit.Id,
                                            Name = bm.PodcastSubscriptionBenefit.Name
                                        }).ToList()
                                })
                                .FirstOrDefaultAsync();
                        }
                    } 
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetPodcastSubscriptionRegistrationsByPodcastEpisodeIdAsync for PodcastEpisodeId: {PodcastEpisodeId}", podcastEpisodeId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription for PodcastEpisodeId: {podcastEpisodeId}. Error: {ex.Message}");
            }
        }
        public async Task<PodcastSubscriptionRegistrationDetailResponseDTO> GetPodcastSubscriptionRegistrationsByPodcastChannelIdAsync(Guid channelId, int accountId)
        {
            try
            {
                return await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                    .Include(psr => psr.PodcastSubscription)
                    .ThenInclude(ps => ps.PodcastSubscriptionBenefitMappings)
                    .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                    .Where(psr => psr.AccountId == accountId && psr.PodcastSubscription.PodcastChannelId == channelId && psr.CancelledAt == null)
                    .Select(psr => new PodcastSubscriptionRegistrationDetailResponseDTO
                    {
                        Id = psr.Id,
                        AccountId = psr.AccountId ?? 0,
                        PodcastSubscriptionId = psr.PodcastSubscriptionId,
                        SubscriptionCycleType = psr.SubscriptionCycleType == null
                        ? null
                        : new SubscriptionCycleTypeDTO
                        {
                            Id = psr.SubscriptionCycleType.Id,
                            Name = psr.SubscriptionCycleType.Name
                        },
                        CurrentVersion = psr.CurrentVersion,
                        IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                        IsIncomeTaken = psr.IsIncomeTaken,
                        LastPaidAt = psr.LastPaidAt,
                        CancelledAt = psr.CancelledAt,
                        CreatedAt = psr.CreatedAt,
                        UpdatedAt = psr.UpdatedAt,
                        PodcastSubscriptionBenefitList = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
                            .Where(psb => psb.Version == psr.CurrentVersion)
                            .Select(bm => new PodcastSubscriptionBenefitDTO
                            {
                                Id = bm.PodcastSubscriptionBenefit.Id,
                                Name = bm.PodcastSubscriptionBenefit.Name
                            }).ToList()
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetPodcastSubscriptionRegistrationsByPodcastChannelIdAsync for PodcastChannelId: {PodcastChannelId}", channelId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription for PodcastChannelId: {channelId}. Error: {ex.Message}");
            }
        }
        public async Task<PodcastSubscriptionRegistrationDetailResponseDTO> GetPodcastSubscriptionRegistrationsByPodcastShowIdAsync(Guid showId , int accountId)
        {
            try
            {
                PodcastSubscriptionRegistrationDetailResponseDTO result = null;
                var show = await GetPodcastShow(showId);
                if(show.PodcastChannelId != null)
                {
                    result = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                    .Include(psr => psr.PodcastSubscription)
                    .ThenInclude(ps => ps.PodcastSubscriptionBenefitMappings)
                    .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                    .Where(psr => psr.AccountId == accountId && psr.PodcastSubscription.PodcastChannelId == show.PodcastChannelId && psr.CancelledAt == null)
                    .Select(psr => new PodcastSubscriptionRegistrationDetailResponseDTO
                    {
                        Id = psr.Id,
                        AccountId = psr.AccountId ?? 0,
                        PodcastSubscriptionId = psr.PodcastSubscriptionId,
                        SubscriptionCycleType = psr.SubscriptionCycleType == null
                        ? null
                        : new SubscriptionCycleTypeDTO
                        {
                            Id = psr.SubscriptionCycleType.Id,
                            Name = psr.SubscriptionCycleType.Name
                        },
                        CurrentVersion = psr.CurrentVersion,
                        IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                        IsIncomeTaken = psr.IsIncomeTaken,
                        LastPaidAt = psr.LastPaidAt,
                        CancelledAt = psr.CancelledAt,
                        CreatedAt = psr.CreatedAt,
                        UpdatedAt = psr.UpdatedAt,
                        PodcastSubscriptionBenefitList = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
                            .Where(psb => psb.Version == psr.CurrentVersion)
                            .Select(bm => new PodcastSubscriptionBenefitDTO
                            {
                                Id = bm.PodcastSubscriptionBenefit.Id,
                                Name = bm.PodcastSubscriptionBenefit.Name
                            }).ToList()
                    })
                    .FirstOrDefaultAsync();
                    if(result == null)
                        result = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                            .Include(psr => psr.PodcastSubscription)
                            .ThenInclude(ps => ps.PodcastSubscriptionBenefitMappings)
                            .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                            .Where(psr => psr.AccountId == accountId && psr.PodcastSubscription.PodcastShowId == showId && psr.CancelledAt == null)
                            .Select(psr => new PodcastSubscriptionRegistrationDetailResponseDTO
                            {
                                Id = psr.Id,
                                AccountId = psr.AccountId ?? 0,
                                PodcastSubscriptionId = psr.PodcastSubscriptionId,
                                SubscriptionCycleType = psr.SubscriptionCycleType == null
                                ? null
                                : new SubscriptionCycleTypeDTO
                                {
                                    Id = psr.SubscriptionCycleType.Id,
                                    Name = psr.SubscriptionCycleType.Name
                                },
                                CurrentVersion = psr.CurrentVersion,
                                IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                                IsIncomeTaken = psr.IsIncomeTaken,
                                LastPaidAt = psr.LastPaidAt,
                                CancelledAt = psr.CancelledAt,
                                CreatedAt = psr.CreatedAt,
                                UpdatedAt = psr.UpdatedAt,
                                PodcastSubscriptionBenefitList = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
                                    .Where(psb => psb.Version == psr.CurrentVersion)
                                    .Select(bm => new PodcastSubscriptionBenefitDTO
                                    {
                                        Id = bm.PodcastSubscriptionBenefit.Id,
                                        Name = bm.PodcastSubscriptionBenefit.Name
                                    }).ToList()
                            })
                            .FirstOrDefaultAsync();
                }
                else
                {
                    result = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Include(psr => psr.PodcastSubscription)
                        .ThenInclude(ps => ps.PodcastSubscriptionBenefitMappings)
                        .ThenInclude(bm => bm.PodcastSubscriptionBenefit)
                        .Where(psr => psr.AccountId == accountId && psr.PodcastSubscription.PodcastShowId == showId && psr.CancelledAt == null)
                        .Select(psr => new PodcastSubscriptionRegistrationDetailResponseDTO
                        {
                            Id = psr.Id,
                            AccountId = psr.AccountId ?? 0,
                            PodcastSubscriptionId = psr.PodcastSubscriptionId,
                            SubscriptionCycleType = psr.SubscriptionCycleType == null
                            ? null
                            : new SubscriptionCycleTypeDTO
                            {
                                Id = psr.SubscriptionCycleType.Id,
                                Name = psr.SubscriptionCycleType.Name
                            },
                            CurrentVersion = psr.CurrentVersion,
                            IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                            IsIncomeTaken = psr.IsIncomeTaken,
                            LastPaidAt = psr.LastPaidAt,
                            CancelledAt = psr.CancelledAt,
                            CreatedAt = psr.CreatedAt,
                            UpdatedAt = psr.UpdatedAt,
                            PodcastSubscriptionBenefitList = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
                                .Where(psb => psb.Version == psr.CurrentVersion)
                                .Select(bm => new PodcastSubscriptionBenefitDTO
                                {
                                    Id = bm.PodcastSubscriptionBenefit.Id,
                                    Name = bm.PodcastSubscriptionBenefit.Name
                                }).ToList()
                        })
                        .FirstOrDefaultAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetPodcastSubscriptionRegistrationsByPodcastShowIdAsync for PodcastShowId: {PodcastShowId}", showId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription for PodcastShowId: {showId}. Error: {ex.Message}");
            }
        }
        public async Task<UserPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO> GetPodcastSubscriptionsByAccountIdAsync(int accountId, List<EpisodeBaseSourceInfoDTO> request)
        {
            try
            {
                //request = request.DistinctBy(r => new { r.PodcastEpisodeId, r.PodcastShowId, r.PodcastChannelId }).ToList();
                var showIds = request.Select(r => r.ShowId).Distinct().ToList();
                var channelIds = request.Where(r => r.ChannelId.HasValue).Select(r => r.ChannelId.Value).Distinct().ToList();

                // Get subscriptions data
                var subscriptionsData = await _podcastSubscriptionGenericRepository.FindAll(
                    includeFunc: ps => ps
                        .Include(ps => ps.PodcastSubscriptionRegistrations)
                        .Include(ps => ps.PodcastSubscriptionBenefitMappings)
                )
                .Where(ps => ps.IsActive && ps.DeletedAt == null &&
                             ps.PodcastSubscriptionRegistrations.Any(psr =>
                                 psr.AccountId == accountId && psr.CancelledAt == null) &&
                             (ps.PodcastShowId.HasValue && showIds.Contains(ps.PodcastShowId.Value) ||
                              (ps.PodcastChannelId.HasValue && channelIds.Contains(ps.PodcastChannelId.Value))))
                .Select(ps => new
                {
                    ps.PodcastShowId,
                    ps.PodcastChannelId,
                    ps.CurrentVersion,
                    BenefitIds = ps.PodcastSubscriptionBenefitMappings
                        .Where(bm => bm.Version == ps.PodcastSubscriptionRegistrations.Where(psr =>
                                 psr.AccountId == accountId && psr.CancelledAt == null).Select(psr => psr.CurrentVersion).FirstOrDefault())
                        .Select(bm => bm.PodcastSubscriptionBenefitId)
                        .ToList()
                })
                .ToListAsync();

                // Create lookup dictionaries for better performance
                var showSubscriptions = subscriptionsData
                    .Where(s => s.PodcastShowId.HasValue)
                    .ToDictionary(s => s.PodcastShowId.Value, s => s.BenefitIds);

                var channelSubscriptions = subscriptionsData
                    .Where(s => s.PodcastChannelId.HasValue)
                    .ToDictionary(s => s.PodcastChannelId.Value, s => s.BenefitIds);

                // Map episodes to benefits
                var episodeBaseBenefitList = request.Select(r => new UserPodcastSubscriptionRegistrationEpisodeBaseBenefitListItemResponseDTO
                {
                    EpisodeId = r.EpisodeId,
                    PodcastSubscriptionBenefitIds = showSubscriptions.ContainsKey(r.ShowId)
                        ? showSubscriptions[r.ShowId]
                        : (r.ChannelId.HasValue && channelSubscriptions.ContainsKey(r.ChannelId.Value)
                            ? channelSubscriptions[r.ChannelId.Value]
                            : new List<int>())
                }).ToList();

                return new UserPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO
                {
                    EpisodeBaseBenefitList = episodeBaseBenefitList ?? new List<UserPodcastSubscriptionRegistrationEpisodeBaseBenefitListItemResponseDTO>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetPodcastSubscriptionsByAccountIdAsync for AccountId: {AccountId}", accountId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscriptions for AccountId: {accountId}. Error: {ex.Message}");
            }
        }
        public async Task<List<PodcastSubscriptionRegistrationMeListItemResponseDTO>> GetAllPodcastSubscriptionRegistrationsByAccountIdAsync(AccountStatusCache account)
        {
            try
            {
                var result = new List<PodcastSubscriptionRegistrationMeListItemResponseDTO>();
                var query = await _podcastSubscriptionRegistrationGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(psr => psr.PodcastSubscription)
                    .ThenInclude(psrct => psrct.PodcastSubscriptionCycleTypePrices)
                    .Include(psr => psr.SubscriptionCycleType))
                    .Where(psr => psr.AccountId == account.Id && psr.CancelledAt == null)
                    .ToListAsync();
                return result = (await Task.WhenAll(query.Select(async psr =>
                {
                    PodcastChannelDTO? channel = null;
                    PodcastShowDTO? show = null;
                    if(psr.PodcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannel(psr.PodcastSubscription.PodcastChannelId.Value);
                    }
                    if (psr.PodcastSubscription.PodcastShowId != null)
                    {
                        show = await GetPodcastShow(psr.PodcastSubscription.PodcastShowId.Value);
                    }
                    return new PodcastSubscriptionRegistrationMeListItemResponseDTO
                    {
                        Id = psr.Id,
                        Account = new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            Email = account.Email,
                            FullName = account.FullName,
                            MainImageFileKey = account.MainImageFileKey
                        },
                        PodcastSubscriptionId = psr.PodcastSubscriptionId,
                        SubscriptionCycleType = psr.SubscriptionCycleType == null
                            ? null
                            : new SubscriptionCycleTypeDTO
                            {
                                Id = psr.SubscriptionCycleType.Id,
                                Name = psr.SubscriptionCycleType.Name
                            },
                        Price = psr.PodcastSubscription.PodcastSubscriptionCycleTypePrices
                            .Where(pctp => pctp.SubscriptionCycleTypeId == psr.SubscriptionCycleTypeId && pctp.Version == psr.CurrentVersion)
                            .Select(pctp => pctp.Price)
                            .FirstOrDefault(),
                        PodcastChannel = channel != null ? new PodcastChannelSnippetResponseDTO
                        {
                            Id = channel.Id,
                            Name = channel.Name,
                            Description = channel.Description,
                            MainImageFileKey = channel.MainImageFileKey
                        } : null ,
                        PodcastShow = show != null ? new PodcastShowSnippetResponseDTO
                        {
                            Id = show.Id,
                            Name = show.Name,
                            Description = show.Description,
                            MainImageFileKey = show.MainImageFileKey,
                            IsReleased = show.IsReleased,
                            ReleaseDate = show.ReleaseDate
                        } : null,
                        CurrentVersion = psr.CurrentVersion,
                        IsAcceptNewestVersionSwitch = psr.IsAcceptNewestVersionSwitch,
                        IsIncomeTaken = psr.IsIncomeTaken,
                        LastPaidAt = psr.LastPaidAt,
                        CancelledAt = psr.CancelledAt,
                        CreatedAt = psr.CreatedAt,
                        UpdatedAt = psr.UpdatedAt
                    };
                }))).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetAllPodcastSubscriptionRegistrationsByAccountIdAsync for AccountId: {AccountId}", account.Id);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription Registrations for AccountId: {account.Id}. Error: {ex.Message}");
            }
        }
        public async Task<SubscribedContentResponseDTO> GetSubscribedContentByAccountIdAsync(int accountId)
        {
            try
            {
                var channelList = new List<PodcastChannelSubscribedContentResponseDTO>();
                var showList = new List<PodcastShowSubscribedContentResponseDTO>();
                var registrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(psr => psr.PodcastSubscription))
                    .Where(psr => psr.AccountId == accountId && psr.CancelledAt == null)
                    .ToListAsync();
                foreach (var registration in registrations)
                {
                    if(registration.PodcastSubscription.PodcastChannelId != null)
                    {
                        var channel = await GetSubscribedPodcastChannel(registration.PodcastSubscription.PodcastChannelId.Value);
                        if(channel != null)
                        {
                            //Console.WriteLine("Found subscribed channel: " + JObject.FromObject(channel));
                            var podcaster = await _accountCachingService.GetAccountStatusCacheById(channel.PodcasterId);
                            channelList.Add(new PodcastChannelSubscribedContentResponseDTO
                            {
                                Id = channel.Id,
                                Name = channel.Name,
                                Description = channel.Description,
                                MainImageFileKey = channel.MainImageFileKey,
                                BackgroundImageFileKey = channel.BackgroundImageFileKey,
                                PodcastCategory = channel.PodcastCategory != null ? new PodcastCategoryDTO
                                {
                                    Id = channel.PodcastCategory.Id,
                                    Name = channel.PodcastCategory.Name,
                                    MainImageFileKey = channel.PodcastCategory.MainImageFileKey
                                } : null,
                                PodcastSubCategory = channel.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                                {
                                    Id = channel.PodcastSubCategory.Id,
                                    Name = channel.PodcastSubCategory.Name,
                                    PodcastCategoryId = channel.PodcastSubCategory.PodcastCategoryId
                                } : null,
                                CurrentStatus = channel.PodcastChannelStatusTrackings.OrderByDescending(channelt => channelt.CreatedAt).Select(channelt => new PodcastStatusDTO
                                {
                                    Id = channelt.PodcastChannelStatusId,
                                    Name = ((PodcastChannelStatusEnum)channelt.PodcastChannelStatusId).ToString()
                                }).FirstOrDefault()!,
                                Hashtags = channel.Hashtags != null ? channel.Hashtags.Select(channelh => new HashtagDTO
                                {
                                    Id = channelh.Id,
                                    Name = channelh.Name 
                                }).ToList() : null,
                                TotalFavorite = channel.TotalFavorite,
                                ListenCount = channel.ListenCount,
                                ShowCount = channel.PodcastShows != null ? channel.PodcastShows.Count(ps => ps.DeletedAt == null) : 0,
                                Podcaster = new AccountSnippetResponseDTO
                                {
                                    Id = podcaster.Id,
                                    FullName = podcaster.FullName,
                                    Email = podcaster.Email,
                                    MainImageFileKey = podcaster.MainImageFileKey
                                },
                                CreatedAt = channel.CreatedAt,
                                UpdatedAt = channel.UpdatedAt
                            });
                            foreach(var showId in channel.PodcastShows)
                            {
                                var show = await GetSubscribedPodcastShow(showId.Id);
                                if (show != null)
                                {
                                    showList.Add(new PodcastShowSubscribedContentResponseDTO
                                    {
                                        Id = show.Id,
                                        Name = show.Name,
                                        Description = show.Description,
                                        MainImageFileKey = show.MainImageFileKey,
                                        TrailerAudioFileKey = show.TrailerAudioFileKey,
                                        TotalFollow = show.TotalFollow,
                                        ListenCount = show.ListenCount,
                                        AverageRating = show.AverageRating,
                                        RatingCount = show.RatingCount,
                                        Copyright = show.Copyright,
                                        IsReleased = show.IsReleased,
                                        Language = show.Language,
                                        EpisodeCount = show.PodcastEpisodes.Count(pe => pe.DeletedAt == null),
                                        PodcastCategory = show.PodcastCategory != null ? new PodcastCategoryDTO
                                        {
                                            Id = show.PodcastCategory.Id,
                                            Name = show.PodcastCategory.Name,
                                            MainImageFileKey = show.PodcastCategory.MainImageFileKey
                                        } : null,
                                        PodcastSubCategory = show.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                                        {
                                            Id = show.PodcastSubCategory.Id,
                                            Name = show.PodcastSubCategory.Name,
                                            PodcastCategoryId = show.PodcastSubCategory.PodcastCategoryId
                                        } : null,
                                        PodcastChannel = show.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                                        {
                                            Id = show.PodcastChannel.Id,
                                            Name = show.PodcastChannel.Name,
                                            Description = show.PodcastChannel.Description,
                                            MainImageFileKey = show.PodcastChannel.MainImageFileKey
                                        } : null,
                                        PodcastShowSubscriptionType = show.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                                        {
                                            Id = show.PodcastShowSubscriptionType.Id,
                                            Name = show.PodcastShowSubscriptionType.Name
                                        } : null,
                                        Podcaster = null,
                                        ReleaseDate = show.ReleaseDate,
                                        TakenDownReason = show.TakenDownReason,
                                        UploadFrequency = show.UploadFrequency,
                                        Hashtags = show.Hashtags != null ? show.Hashtags.Select(showh => new HashtagDTO
                                        {
                                            Id = showh.Id,
                                            Name = showh.Name
                                        }).ToList() : null,
                                        CreatedAt = show.CreatedAt,
                                        UpdatedAt = show.UpdatedAt,
                                        CurrentStatus = show.PodcastShowStatusTrackings.OrderByDescending(showt => showt.CreatedAt).Select(showt => new PodcastStatusDTO
                                        {
                                            Id = showt.PodcastShowStatusId,
                                            Name = ((PodcastShowStatusEnum)showt.PodcastShowStatusId).ToString()
                                        }).FirstOrDefault()!
                                    });
                                }
                            }
                        }
                    } else if(registration.PodcastSubscription.PodcastShowId != null)
                    {
                        var show = await GetSubscribedPodcastShow(registration.PodcastSubscription.PodcastShowId.Value);
                        if(show != null)
                        {
                            var podcaster = await _accountCachingService.GetAccountStatusCacheById(show.PodcasterId);
                            showList.Add(new PodcastShowSubscribedContentResponseDTO
                            {
                                Id = show.Id,
                                Name = show.Name,
                                Description = show.Description,
                                MainImageFileKey = show.MainImageFileKey,
                                TrailerAudioFileKey = show.TrailerAudioFileKey,
                                TotalFollow = show.TotalFollow,
                                ListenCount = show.ListenCount,
                                AverageRating = show.AverageRating,
                                RatingCount = show.RatingCount,
                                Copyright = show.Copyright,
                                IsReleased = show.IsReleased,
                                Language = show.Language,
                                EpisodeCount = show.PodcastEpisodes.Count(pe => pe.DeletedAt == null),
                                PodcastCategory = show.PodcastCategory != null ? new PodcastCategoryDTO
                                {
                                    Id = show.PodcastCategory.Id,
                                    Name = show.PodcastCategory.Name,
                                    MainImageFileKey = show.PodcastCategory.MainImageFileKey
                                } : null,
                                PodcastSubCategory = show.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                                {
                                    Id = show.PodcastSubCategory.Id,
                                    Name = show.PodcastSubCategory.Name,
                                    PodcastCategoryId = show.PodcastSubCategory.PodcastCategoryId
                                } : null,
                                PodcastChannel = show.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                                {
                                    Id = show.PodcastChannel.Id,
                                    Name = show.PodcastChannel.Name,
                                    Description = show.PodcastChannel.Description,
                                    MainImageFileKey = show.PodcastChannel.MainImageFileKey
                                } : null,
                                PodcastShowSubscriptionType = show.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                                {
                                    Id = show.PodcastShowSubscriptionType.Id,
                                    Name = show.PodcastShowSubscriptionType.Name
                                } : null,
                                Podcaster = null,
                                ReleaseDate = show.ReleaseDate,
                                TakenDownReason = show.TakenDownReason,
                                UploadFrequency = show.UploadFrequency,
                                Hashtags = show.Hashtags != null ? show.Hashtags.Select(showh => new HashtagDTO
                                {
                                    Id = showh.Id,
                                    Name = showh.Name
                                }).ToList() : null,
                                CreatedAt = show.CreatedAt,
                                UpdatedAt = show.UpdatedAt,
                                CurrentStatus = show.PodcastShowStatusTrackings.OrderByDescending(showt => showt.CreatedAt).Select(showt => new PodcastStatusDTO
                                {
                                    Id = showt.PodcastShowStatusId,
                                    Name = ((PodcastShowStatusEnum)showt.PodcastShowStatusId).ToString()
                                }).FirstOrDefault()!
                            });
                        }
                    }
                }
                return new SubscribedContentResponseDTO
                {
                    PodcastChannelList = channelList.DistinctBy(c => c.Id).ToList(),
                    PodcastShowList = showList.DistinctBy(s => s.Id).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetSubscribedContentByAccountIdAsync for AccountId: {AccountId}", accountId);
                throw new HttpRequestException($"Error while retrieving Subscribed Content for AccountId: {accountId}. Error: {ex.Message}");
            }
        }
        public async Task<PodcastSubscriptionDashboardResponseDTO> GetPodcastSubscriptionDashboardByPodcastChannelIdAsync(Guid podcastChannelId)
        {
            try
            {
                var config = await GetActiveSystemConfigProfile();
                //int incomeReleaseDelayDay = config.PodcastSubscriptionConfigs.FirstOrDefault()?.IncomeTakenDelayDays ?? 7;
                int incomeReleaseDelayDay = 0;
                var result = new PodcastSubscriptionDashboardResponseDTO();
                result.Last30DayList = new List<PodcastSubscriptionLast30DashboardListItemResponseDTO>();
                var subscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(ps => ps.PodcastSubscriptionRegistrations))
                    .Where(ps => ps.PodcastChannelId == podcastChannelId)
                    .ToListAsync();
                for(int i = 30; i >= 0; i--)
                {
                    var date = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()).AddDays(-i);
                    Console.WriteLine("Calculating for date: " + date);
                    var registrations = subscriptions
                        .SelectMany(ps => ps.PodcastSubscriptionRegistrations)
                        .Where(psr => DateOnly.FromDateTime(psr.LastPaidAt) == date && psr.IsIncomeTaken);
                    Console.WriteLine("Found registrations count: " + registrations.Count());
                    Console.WriteLine("Registrations: " + JArray.FromObject(registrations.Select(r => r.Id)));
                    decimal dailyAmount = 0;
                    foreach (var registration in registrations)
                    {
                        var transactions = await GetPodcastSubscriptionTransactionByRegistrationId(registration.Id);
                        if (transactions != null)
                        {
                            dailyAmount += transactions.Where(t => DateOnly.FromDateTime(t.CreatedAt).AddDays(-incomeReleaseDelayDay) >= date).Sum(t => t.Amount);
                            Console.WriteLine(date + " - Registration ID: " + registration.Id + " - Daily Amount after adding: " + dailyAmount);
                        }
                            
                    }
                    result.Last30DayList.Add(new PodcastSubscriptionLast30DashboardListItemResponseDTO
                    {
                        Date = date,
                        Amount = dailyAmount
                    });
                }
                result.LastMonthTotalAmount = result.Last30DayList
                    .Where(item => item.Date >= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()).AddDays(-30))
                    .Sum(item => item.Amount);
                var threeMonthAgoRegistrations = subscriptions
                    .SelectMany(ps => ps.PodcastSubscriptionRegistrations)
                    .Where(psr => psr.LastPaidAt >= _dateHelper.GetNowByAppTimeZone().AddMonths(-3));
                foreach (var registration in threeMonthAgoRegistrations)
                {
                    var transactions = await GetPodcastSubscriptionTransactionByRegistrationId(registration.Id);
                    if (transactions != null)
                        result.Last3MonthTotalAmount += transactions.Where(t => DateOnly.FromDateTime(t.CreatedAt) >= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone().AddMonths(-3))).Sum(t => t.Amount);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetPodcastSubscriptionDashboardByPodcastChannelIdAsync for PodcastChannelId: {PodcastChannelId}", podcastChannelId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription Dashboard for PodcastChannelId: {podcastChannelId}. Error: {ex.Message}");
            }
        }
        public async Task<PodcastSubscriptionDashboardResponseDTO> GetPodcastSubscriptionDashboardByPodcastShowIdAsync(Guid podcastShowId)
        {
            try
            {
                var config = await GetActiveSystemConfigProfile();
                //int incomeReleaseDelayDay = config.PodcastSubscriptionConfigs.FirstOrDefault()?.IncomeTakenDelayDays ?? 7;
                int incomeReleaseDelayDay = 0;
                var result = new PodcastSubscriptionDashboardResponseDTO();
                result.Last30DayList = new List<PodcastSubscriptionLast30DashboardListItemResponseDTO>();
                var subscriptions = await _podcastSubscriptionGenericRepository.FindAll(
                    includeFunc: function => function
                    .Include(ps => ps.PodcastSubscriptionRegistrations))
                    .Where(ps => ps.PodcastShowId == podcastShowId)
                    .ToListAsync();
                for (int i = 30; i >= 0; i--)
                {
                    var date = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()).AddDays(-i);
                    Console.WriteLine("Calculating for date: " + date);
                    var registrations = subscriptions
                        .SelectMany(ps => ps.PodcastSubscriptionRegistrations)
                        .Where(psr => DateOnly.FromDateTime(psr.LastPaidAt) == date && psr.IsIncomeTaken);
                    Console.WriteLine("Found registrations count: " + registrations.Count());
                    //Console.WriteLine($"Registrations: {JArray.FromObject(registrations)}");
                    decimal dailyAmount = 0;
                    foreach (var registration in registrations)
                    {
                        var transactions = await GetPodcastSubscriptionTransactionByRegistrationId(registration.Id);
                        if(transactions != null)
                            dailyAmount += transactions.Where(t => DateOnly.FromDateTime(t.CreatedAt).AddDays(-incomeReleaseDelayDay) >= date).Sum(t => t.Amount);
                    }
                    result.Last30DayList.Add(new PodcastSubscriptionLast30DashboardListItemResponseDTO
                    {
                        Date = date,
                        Amount = dailyAmount
                    });
                }
                result.LastMonthTotalAmount = result.Last30DayList
                    .Where(item => item.Date >= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()).AddDays(-30))
                    .Sum(item => item.Amount);
                var threeMonthAgoRegistrations = subscriptions
                    .SelectMany(ps => ps.PodcastSubscriptionRegistrations)
                    .Where(psr => psr.LastPaidAt >= _dateHelper.GetNowByAppTimeZone().AddMonths(-3));
                foreach (var registration in threeMonthAgoRegistrations)
                {
                    var transactions = await GetPodcastSubscriptionTransactionByRegistrationId(registration.Id);
                    if(transactions != null)
                        result.Last3MonthTotalAmount += transactions.Where(t => DateOnly.FromDateTime(t.CreatedAt) >= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone().AddMonths(-3))).Sum(t => t.Amount);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while GetPodcastSubscriptionDashboardByPodcastChannelIdAsync for PodcastShowId: {PodcastShowId}", podcastShowId);
                throw new HttpRequestException($"Error while retrieving Podcast Subscription Dashboard for PodcastShowId: {podcastShowId}. Error: {ex.Message}");
            }
        }
        public async Task<PodcastChannelDTO?> GetPodcastChannelWithAccountId(int accountId, Guid podcastChannelId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastChannelOfAccount",
                            QueryType = "findall",
                            EntityType = "PodcastChannel",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = podcastChannelId,
                                    PodcasterId = accountId
                                },
                                include = "PodcastChannelStatusTrackings"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);
            var realResult = result.Results?["podcastChannelOfAccount"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                ? podcastChannelArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<PodcastChannelDTO>() : null;
        }
        private async Task<(bool isValid, string errorMessage)> ValidateEpisode(Guid podcastEpisodeId)
        {
            var episode = await GetPodcastEpisode(podcastEpisodeId);
            if (episode == null)
            {
                return (false, $"Podcast episode is not found");
            }
            var (isValid, errorMessage) = await ValidateShow(episode.PodcastShowId, podcastEpisodeId);
            if (!isValid)
            {
                return (isValid, errorMessage);
            }
            if (episode.DeletedAt != null)
            {
                return (false, $"Podcast episode has already been deleted");
            }
            var episodeStatusId = episode.PodcastEpisodeStatusTrackings.OrderByDescending(es => es.CreatedAt).Select(es => es.PodcastEpisodeStatusId).First();
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.Draft)
            {
                return (false, $"Podcast episode is in Draft status");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.PendingReview)
            {
                return (false, $"Podcast episode is pending review");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.PendingEditRequired)
            {
                return (false, $"Podcast episode is pending edit required");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown)
            {
                return (false, $"Podcast episode has been taken down");
            }
            if (episodeStatusId == (int)PodcastEpisodeStatusEnum.Removed)
            {
                return (false, $"Podcast episode has been removed");
            }
            return (true, string.Empty);
        }
        private async Task<(bool isValid, string errorMessage)> ValidateShow(Guid podcastShowId, Guid? podcastEpisodeId = null)
        {
            var insideMessage = podcastEpisodeId != null ? $" for Episode Id: {podcastEpisodeId}" : string.Empty;
            var show = await GetPodcastShow(podcastShowId);
            if (show == null)
            {
                return (false, $"Podcast show is not found {insideMessage}");
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
                return (false, $"Podcast show has already been deleted {insideMessage}");
            }
            var showStatusId = show.PodcastShowStatusTrackings.OrderByDescending(ss => ss.CreatedAt).Select(ss => ss.PodcastShowStatusId).First();
            if (showStatusId == (int)PodcastShowStatusEnum.Draft)
            {
                return (false, $"Podcast show is in Draft status {insideMessage}");
            }
            if (showStatusId == (int)PodcastShowStatusEnum.TakenDown)
            {
                return (false, $"Podcast show has been taken down {insideMessage}");
            }
            if (showStatusId == (int)PodcastShowStatusEnum.Removed)
            {
                return (false, $"Podcast show has been removed {insideMessage}");
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
                return (false, $"Podcast channel is not found {insideMessage}");
            }
            if (channel.DeletedAt != null)
            {
                return (false, $"Podcast channel has already been deleted {insideMessage}");
            }
            var channelStatusId = channel.PodcastChannelStatusTrackings.OrderByDescending(cs => cs.CreatedAt).Select(cs => cs.PodcastChannelStatusId).First();
            if (channelStatusId == (int)PodcastChannelStatusEnum.Unpublished)
            {
                return (false, $"Podcast channel is in Unpublished status {insideMessage}");
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
                                include = "PodcastEpisodeStatusTrackings"
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
        public async Task<AccountDTO?> GetAccount(int accountId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                    {
                        Key = "account",
                        QueryType = "findbyid",
                        EntityType = "Account",
                        Parameters = JObject.FromObject(new
                        {
                            id = accountId
                        }),
                        Fields = new[] {
                            "Id",
                            "Email",
                            "Password",
                            "RoleId",
                            "FullName",
                            "Dob",
                            "Gender",
                            "Address",
                            "Phone",
                            "Balance"
                        }
                    }
                }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);

            if (result.Results["account"] == null) return null;

            return (result.Results["account"] as JObject).ToObject<AccountDTO>();
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
                                include = "PodcastChannelStatusTrackings"
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
                                include = "PodcastShowStatusTrackings"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.First as JObject
                : null;
            //Console.WriteLine("Real Result: " + realResult);
            return realResult != null ? realResult.ToObject<PodcastShowDTO>() : null;
        }
        public async Task<PodcastChannelSubscribedDTO?> GetSubscribedPodcastChannel(Guid podcastChannelId)
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
                                include = "PodcastChannelStatusTrackings, PodcastChannelHashtags, PodcastCategory, PodcastSubCategory, PodcastShows"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastChannel"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                ? podcastChannelArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<PodcastChannelSubscribedDTO>() : null;
        }
        public async Task<PodcastShowSubscribedDTO?> GetSubscribedPodcastShow(Guid podcastShowId)
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
                                include = "PodcastShowStatusTrackings, PodcastShowHashtags, PodcastCategory, PodcastSubCategory, PodcastShowSubscriptionType, PodcastChannel, PodcastEpisodes"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.First as JObject
                : null;
            //Console.WriteLine("Real Result: " + realResult);
            return realResult != null ? realResult.ToObject<PodcastShowSubscribedDTO>() : null;
        }
        public async Task<List<PodcastShowDTO>> GetPodcastShowByPodcastChannelId(Guid podcastChannelId)
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
                                    PodcastChannelId = podcastChannelId
                                },
                                include = "PodcastShowStatusTrackings"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastShow"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray as JArray
                : null;
            return realResult != null ? realResult.ToObject<List<PodcastShowDTO>>() : null;
        }
        public async Task<List<PodcastChannelDTO>?> GetPodcastChannelsWithAccountId(int accountId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastChannelOfAccount",
                            QueryType = "findall",
                            EntityType = "PodcastChannel",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcasterId = accountId
                                },
                                include = "PodcastChannelStatusTrackings"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastChannelOfAccount"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                ? podcastChannelArray.ToObject<List<PodcastChannelDTO>>()
                : null;
        }
        public async Task<List<PodcastSubscriptionTransactionDTO>?> GetPodcastSubscriptionTransactionByRegistrationId(Guid registrationId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastSubscriptionTransaction",
                            QueryType = "findall",
                            EntityType = "PodcastSubscriptionTransaction",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcastSubscriptionRegistrationId = registrationId,
                                    TransactionTypeId =(int)TransactionTypeEnum.PodcasterSubscriptionIncome,
                                    TransactionStatusId = (int)TransactionStatusEnum.Success
                                }
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("TransactionService", batchRequest);

            return result.Results?["podcastSubscriptionTransaction"] is JArray podcastSubscriptionTransactionArray && podcastSubscriptionTransactionArray.Count > 0
                ? podcastSubscriptionTransactionArray.ToObject<List<PodcastSubscriptionTransactionDTO>>()
                : null;
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
        public async Task<bool> ValidatePodcastSubscriptionRegistration(int accountId, Guid PodcastSubscriptionRegistrationId)
        {
            var podcastSubscriptionRegistration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                .Where(psr => psr.Id == PodcastSubscriptionRegistrationId && psr.AccountId == accountId)
                .FirstOrDefaultAsync();
            return podcastSubscriptionRegistration != null;
        }
        public async Task<bool> ValidatePodcastSubscriptionAccess(int accountId, int PodcastSubscriptionId)
        {
            var podcastShowOrChannelIds = await _podcastSubscriptionGenericRepository.FindAll()
                .Where(ps => ps.Id == PodcastSubscriptionId)
                .Select(ps => new { ps.PodcastShowId, ps.PodcastChannelId })
                .FirstOrDefaultAsync();

            if (podcastShowOrChannelIds == null)
                return false;

            if (podcastShowOrChannelIds.PodcastShowId.HasValue)
            {
                var result = await GetPodcastShowWithAccountId(accountId, podcastShowOrChannelIds.PodcastShowId.Value);
                if (result != null)
                {
                    return true;
                }
            }
            if (podcastShowOrChannelIds.PodcastChannelId.HasValue)
            {
                var result = await GetPodcastChannelWithAccountId(accountId, podcastShowOrChannelIds.PodcastChannelId.Value);
                if (result != null)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task<PodcastShowDTO?> GetPodcastShowWithAccountId(int accountId, Guid podcastShowId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastShowOfAccount",
                            QueryType = "findall",
                            EntityType = "PodcastShow",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    Id = podcastShowId,
                                    PodcasterId = accountId
                                },
                                include = "PodcastShowStatusTrackings"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            var realResult = result.Results?["podcastShowOfAccount"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.First as JObject
                : null;
            return realResult != null ? realResult.ToObject<PodcastShowDTO>() : null;
        }
    }
}
