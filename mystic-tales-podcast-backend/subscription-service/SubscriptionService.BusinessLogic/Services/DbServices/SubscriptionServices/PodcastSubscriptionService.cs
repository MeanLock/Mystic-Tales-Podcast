using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Podcast;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.ActivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscriptionRegistration;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelShowSubscriptionDmcaRemoveShowForce;
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
using SubscriptionService.BusinessLogic.DTOs.Subscription;
using SubscriptionService.BusinessLogic.Enums.Kafka;
using SubscriptionService.BusinessLogic.Enums.Podcast;
using SubscriptionService.BusinessLogic.Enums.Subscription;
using SubscriptionService.BusinessLogic.Enums.Transaction;
using SubscriptionService.BusinessLogic.Helpers.DateHelpers;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities.SqlServer;
using SubscriptionService.DataAccess.Repositories.interfaces;
using SubscriptionService.Infrastructure.Models.Kafka;
using SubscriptionService.Infrastructure.Services.Kafka;

namespace SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices
{
    public class PodcastSubscriptionService
    {
        private readonly IGenericRepository<PodcastSubscription> _podcastSubscriptionGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionCycleTypePrice> _podcastSubscriptionCycleTypePriceGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionBenefitMapping> _podcastSubscriptionBenefitMappingGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionRegistration> _podcastSubscriptionRegistrationGenericRepository;

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
                    IsActive = ps.DeletedAt == null,
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
                    IsActive = ps.DeletedAt == null,
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
                    var cycleTypePrices = new List<PodcastSubscriptionCycleTypePrice>();
                    var benefitMappings = new List<PodcastSubscriptionBenefitMapping>();
                    var show = new JObject();
                    var channel = new JObject();
                    if (parameter.PodcastShowId != null)
                    {
                        show = await GetPodcastShowWithAccountId(parameter.AccountId, parameter.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastShow with Id: {parameter.PodcastShowId}");
                        }
                        var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .FirstOrDefaultAsync(ps => ps.PodcastShowId == parameter.PodcastShowId && ps.DeletedAt == null);
                        if (existPodcastSubscription != null)
                        {
                            throw new Exception($"A Podcast Subscription already exists for PodcastShow Id: {parameter.PodcastShowId}");
                        }
                        var showValidation = await ValidateShow(parameter.PodcastShowId.Value);
                        if (!showValidation.isValid)
                        {
                            throw new Exception(showValidation.errorMessage);
                        }
                    }
                    if (parameter.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, parameter.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastChanne; with Id: {parameter.PodcastChannelId}");
                        }
                        var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .FirstOrDefaultAsync(ps => ps.PodcastChannelId == parameter.PodcastChannelId && ps.DeletedAt == null);
                        if (existPodcastSubscription != null)
                        {
                            throw new Exception($"An Active Podcast Subscription already exists for PodcastChannel Id: {parameter.PodcastChannelId}");
                        }
                        var channelValidation = await ValidateChannel(parameter.PodcastChannelId.Value);
                        if (!channelValidation.isValid)
                        {
                            throw new Exception(channelValidation.errorMessage);
                        }
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
                        cycleTypePrices.Add(cycleTypePriceResult);
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
                        if (benefitMappings == null)
                        {
                            benefitMappings.Add(benefitMappingResult);
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "PodcastSubscriptionId", podcastSubscription.Id },
                        { "Name", podcastSubscription.Name },
                        { "Description", podcastSubscription.Description },
                        { "PodcastShowId", podcastSubscription.PodcastShowId },
                        { "PodcastChannelId", podcastSubscription.PodcastChannelId },
                        { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(cycleTypePrices) },
                        { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(benefitMappings) },
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
        public async Task<PodcastSubscriptionDetailResponseDTO?> GetPodcastSubscriptionByIdAsync(int podcastSubscriptionId)
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
                .Select(ps => new PodcastSubscriptionDetailResponseDTO
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    Description = ps.Description,
                    PodcastShowId = ps.PodcastShowId,
                    PodcastChannelId = ps.PodcastChannelId,
                    IsActive = ps.DeletedAt == null,
                    CurrentVersion = ps.CurrentVersion,
                    DeletedAt = ps.DeletedAt,
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt,
                    PodcastSubscriptionCycleTypePriceList = ps.PodcastSubscriptionCycleTypePrices
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
                    PodcastSubscriptionRegistrationList = ps.PodcastSubscriptionRegistrations
                        .Select(sr => new DTOs.PodcastSubscription.ListItems.PodcastSubscriptionRegistrationListItemResponseDTO
                        {
                            Id = sr.Id,
                            AccountId = sr.AccountId ?? 0,
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
                        }).ToList()
                })
                .FirstOrDefaultAsync();
            if (podcastSubscription == null)
            {
                _logger.LogWarning("No Podcast subscription with ID {PodcastSubscriptionId} found.", podcastSubscriptionId);
                return null;
            }
            return podcastSubscription;
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
                    var cycleTypePrices = null as List<PodcastSubscriptionCycleTypePrice>;
                    var benefitMappings = null as List<PodcastSubscriptionBenefitMapping>;
                    var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.DeletedAt == null);
                    if (existPodcastSubscription == null)
                    {
                        _logger.LogWarning("No Active Podcast Subscription exists for PodcastSubscription Id: {PodcastSubscriptionId}", parameter.PodcastSubscriptionId);
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }

                    var show = new JObject();
                    var channel = new JObject();
                    if (existPodcastSubscription.PodcastShowId != null)
                    {
                        show = await GetPodcastShowWithAccountId(parameter.AccountId, existPodcastSubscription.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to update PodcastSubscription for PodcastShow with Id: {existPodcastSubscription.PodcastShowId}");
                        }
                        var showValidation = await ValidateShow(existPodcastSubscription.PodcastShowId.Value);
                        if (!showValidation.isValid)
                        {
                            throw new Exception(showValidation.errorMessage);
                        }
                    }
                    if (existPodcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, existPodcastSubscription.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to update PodcastSubscription for PodcastChannel with Id: {existPodcastSubscription.PodcastChannelId}");
                        }
                        var channelValidation = await ValidateChannel(existPodcastSubscription.PodcastChannelId.Value);
                        if (!channelValidation.isValid)
                        {
                            throw new Exception(channelValidation.errorMessage);
                        }
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
                        if (cycleTypePrices == null)
                        {
                            cycleTypePrices.Add(cycleTypePriceResult);
                        }
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
                        if (benefitMappings == null)
                        {
                            benefitMappings.Add(benefitMappingResult);
                        }
                    }

                    var updateRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(sr => sr.PodcastSubscriptionId == parameter.PodcastSubscriptionId && sr.CancelledAt == null)
                        .ToListAsync();

                    foreach (var registration in updateRegistrations)
                    {
                        registration.IsAcceptNewestVersionSwitch = false;
                        registration.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(registration.Id, registration);
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "PodcastSubscriptionId", podcastSubscription.Id },
                        { "Name", podcastSubscription.Name },
                        { "Description", podcastSubscription.Description },
                        { "PodcastSubscriptionCycleTypePriceList", JArray.FromObject(cycleTypePrices) },
                        { "PodcastSubscriptionBenefitMappingList", JArray.FromObject(benefitMappings) },
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
                    var show = new JObject();
                    var channel = new JObject();
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
                        var showValidation = await ValidateShow(existPodcastSubscription.PodcastShowId.Value);
                        if (!showValidation.isValid)
                        {
                            throw new Exception(showValidation.errorMessage);
                        }
                    }
                    if (existPodcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, existPodcastSubscription.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to delete PodcastSubscription for PodcastChannel with Id: {existPodcastSubscription.PodcastChannelId}");
                        }
                        var channelValidation = await ValidateChannel(existPodcastSubscription.PodcastChannelId.Value);
                        if (!channelValidation.isValid)
                        {
                            throw new Exception(channelValidation.errorMessage);
                        }
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

                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.DeletedAt == null);
                    if (podcastSubscription == null)
                    {
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }

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
                                    var profitRate = systemConfig?["PodcastSubscriptionConfig"]
                                        .Where(psc => psc["SubscriptionCycleTypeId"].ToObject<int>() == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                                        .Select(psc => psc["ProfitRate"]?.ToObject<decimal>() ?? 0)
                                        .FirstOrDefault();
                                    var incomeTakenDelayDays = systemConfig?["PodcastSubscriptionConfig"]
                                        .Where(psc => psc["SubscriptionCycleTypeId"].ToObject<int>() == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                                        .Select(psc => psc["IncomeTakenDelayDays"]?.ToObject<int>() ?? 0)
                                        .FirstOrDefault();

                                    if (podcastSubscriptionRegistration.LastPaidAt.AddDays((double)incomeTakenDelayDays) < _dateHelper.GetNowByAppTimeZone())
                                    {
                                        podcastSubscriptionRegistration.IsIncomeTaken = true;
                                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(podcastSubscriptionRegistration.Id, podcastSubscriptionRegistration);

                                        var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
                                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                                            .Select(ptcp => ptcp.Price)
                                            .FirstOrDefault();

                                        var amount = originalPrice - originalPrice * profitRate;
                                        var profit = originalPrice * profitRate;

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
                                    await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(podcastSubscriptionRegistration.Id, podcastSubscriptionRegistration);
                                }
                            }
                        }

                        var newRegistration = new PodcastSubscriptionRegistration
                        {
                            AccountId = parameter.AccountId,
                            PodcastSubscriptionId = parameter.PodcastSubscriptionId,
                            SubscriptionCycleTypeId = parameter.SubscriptionCycleTypeId,
                            CurrentVersion = existRegistration.CurrentVersion,
                            IsAcceptNewestVersionSwitch = null,
                            IsIncomeTaken = false,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var registrationResult = await _podcastSubscriptionRegistrationGenericRepository.CreateAsync(newRegistration);
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
                .Select(psr => new PodcastSubscriptionRegistrationListItemResponseDTO
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
                    UpdatedAt = psr.UpdatedAt
                })
                .ToListAsync();
            if (podcastSubscriptionsRegistrations == null || podcastSubscriptionsRegistrations.Count == 0)
            {
                _logger.LogWarning("No Podcast subscription registrations found for Account ID {AccountId}.", accountId);
                return null;
            }
            return podcastSubscriptionsRegistrations;
        }
        public async Task<List<PodcastSubscriptionRegistrationListItemResponseDTO>> GetShowPodcastSubscriptionsRegistrationsByAccountIdAsync(int accountId)
        {
            var podcastSubscriptionsRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll(
                includeFunc: psr => psr
                    .Include(ps => ps.PodcastSubscription)
                    .Include(sct => sct.SubscriptionCycleType)
                )
                .Where(psr => psr.AccountId == accountId && psr.CancelledAt == null && psr.PodcastSubscription.PodcastShowId != null)
                .Select(psr => new PodcastSubscriptionRegistrationListItemResponseDTO
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
                    UpdatedAt = psr.UpdatedAt
                })
                .ToListAsync();
            if (podcastSubscriptionsRegistrations == null || podcastSubscriptionsRegistrations.Count == 0)
            {
                _logger.LogWarning("No Podcast subscription registrations found for Account ID {AccountId}.", accountId);
                return null;
            }
            return podcastSubscriptionsRegistrations;
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
                    PodcastSubscriptionBenefit = psr.PodcastSubscription.PodcastSubscriptionBenefitMappings
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
                    var show = new JObject();
                    var channel = new JObject();
                    if (podcastSubscription.PodcastShowId != null)
                    {
                        show = await GetPodcastShowWithAccountId(parameter.AccountId, podcastSubscription.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to activate PodcastSubscription for PodcastShow with Id: {podcastSubscription.PodcastShowId}");
                        }
                        var showValidation = await ValidateShow(podcastSubscription.PodcastShowId.Value);
                        if (!showValidation.isValid)
                        {
                            throw new Exception(showValidation.errorMessage);
                        }
                    }
                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannelWithAccountId(parameter.AccountId, podcastSubscription.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to activate PodcastSubscription for PodcastChannel with Id: {podcastSubscription.PodcastChannelId}");
                        }
                        var channelValidation = await ValidateChannel(podcastSubscription.PodcastChannelId.Value);
                        if (!channelValidation.isValid)
                        {
                            throw new Exception(channelValidation.errorMessage);
                        }
                    }

                    //if (show.Count > 0 && show.HasValues)
                    //{
                    //    var result = await _podcastSubscriptionGenericRepository.FindAll().
                    //        Where(ps => ps.PodcastChannelId.Equals(show["PodcastChannelId"]) && ps.IsActive == true && ps.DeletedAt == null)
                    //        .FirstOrDefaultAsync();
                    //    if(result != null)
                    //    {
                    //        throw new Exception($"An Active Channel Podcast Subscription exists for PodcastShow Id: {podcastSubscription.PodcastShowId}");
                    //    }
                    //}

                    var existingActivePodcastSubscriptions = await _podcastSubscriptionGenericRepository.FindAll()
                        .Where(ps => ps.IsActive)
                        .ToListAsync();
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
                            if (!registration.IsIncomeTaken)
                            {
                                var amount = existingSubscription.PodcastSubscriptionCycleTypePrices
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
                        var showValidation = await ValidateShow(podcastSubscription.PodcastShowId.Value);
                        if (!showValidation.isValid)
                        {
                            throw new Exception(showValidation.errorMessage);
                        }
                    }
                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        var isValid = await GetPodcastChannelWithAccountId(parameter.AccountId, podcastSubscription.PodcastChannelId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to deactivate PodcastSubscription for PodcastChannel with Id: {podcastSubscription.PodcastChannelId}");
                        }
                        var channelValidation = await ValidateChannel(podcastSubscription.PodcastChannelId.Value);
                        if (!channelValidation.isValid)
                        {
                            throw new Exception(channelValidation.errorMessage);
                        }
                    }
                    podcastSubscription.IsActive = false;
                    podcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _podcastSubscriptionGenericRepository.UpdateAsync(podcastSubscription.Id, podcastSubscription);

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
                    var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                        .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                        .Where(ps => ps.Id == podcastSubscriptionRegistration.PodcastSubscriptionId && ps.DeletedAt == null)
                        .FirstOrDefaultAsync();

                    var systemConfig = await GetActiveSystemConfigProfile();
                    var profitRate = systemConfig?["PodcastSubscriptionConfig"]
                        .Where(psc => psc["SubscriptionCycleTypeId"].ToObject<int>() == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                        .Select(psc => psc["ProfitRate"]?.ToObject<decimal>() ?? 0)
                        .FirstOrDefault();
                    var incomeTakenDelayDays = systemConfig?["PodcastSubscriptionConfig"]
                        .Where(psc => psc["SubscriptionCycleTypeId"].ToObject<int>() == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                        .Select(psc => psc["IncomeTakenDelayDays"]?.ToObject<int>() ?? 0)
                        .FirstOrDefault();

                    if (podcastSubscriptionRegistration.LastPaidAt.AddDays((double)incomeTakenDelayDays) < _dateHelper.GetNowByAppTimeZone())
                    {
                        podcastSubscriptionRegistration.IsIncomeTaken = true;
                        await _podcastSubscriptionRegistrationGenericRepository.UpdateAsync(podcastSubscriptionRegistration.Id, podcastSubscriptionRegistration);

                        var originalPrice = podcastSubscription.PodcastSubscriptionCycleTypePrices
                            .Where(ptcp => ptcp.SubscriptionCycleTypeId == podcastSubscriptionRegistration.SubscriptionCycleTypeId)
                            .Select(ptcp => ptcp.Price)
                            .FirstOrDefault();

                        var amount = originalPrice - originalPrice * profitRate;
                        var profit = originalPrice * profitRate;

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
        public async Task PodcastSubscriptionRegistrationRenewalByHourAsync()
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
                        if (registration.IsAcceptNewestVersionSwitch == null)
                        {
                            var podcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                                .Include(ps => ps.PodcastSubscriptionCycleTypePrices)
                                .Where(ps => ps.Id == registration.PodcastSubscriptionId && ps.DeletedAt == null && ps.IsActive)
                                .FirstOrDefaultAsync();
                            registration.LastPaidAt = _dateHelper.GetNowByAppTimeZone();
                            var renewalRequestData = new JObject
                            {
                                { "PodcastSubscriptionRegistrationId", registration.Id },
                                { "Profit", null },
                                { "AccountId", registration.AccountId },
                                { "PodcasterId", null },
                                { "Amount", podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId
                                    && ptcp.Version == registration.CurrentVersion)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault() },
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
                            registration.CurrentVersion = podcastSubscription.CurrentVersion;
                            registration.LastPaidAt = _dateHelper.GetNowByAppTimeZone();
                            var renewalRequestData = new JObject
                            {
                                { "PodcastSubscriptionRegistrationId", registration.Id },
                                { "Profit", null },
                                { "AccountId", registration.AccountId },
                                { "PodcasterId", null },
                                { "Amount", podcastSubscription.PodcastSubscriptionCycleTypePrices
                                    .Where(ptcp => ptcp.SubscriptionCycleTypeId == registration.SubscriptionCycleTypeId
                                    && ptcp.Version == podcastSubscription.CurrentVersion)
                                    .Select(ptcp => ptcp.Price)
                                    .FirstOrDefault() },
                                { "TransactionTypeId", (int)TransactionTypeEnum.CustomerSubscriptionCyclePayment }
                            };
                            var renewalMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(
                                topic: KafkaTopicEnum.PaymentProcessingDomain,
                                requestData: renewalRequestData,
                                sagaInstanceId: null,
                                messageName: "podcast-subscription-payment-flow");
                            await _messagingService.SendSagaMessageAsync(renewalMessage, null);
                        }
                        if (registration.IsAcceptNewestVersionSwitch == false)
                        {
                            registration.CancelledAt = _dateHelper.GetNowByAppTimeZone();
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
        public async Task PodcastSubscriptionIncomeCheckingByHourAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var systemConfig = await GetActiveSystemConfigProfile();
                    var podcastSubscriptionConfig = systemConfig["PodcastSubscriptionConfigs"].ToList();

                    var registrationsList = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(psr => !psr.IsIncomeTaken
                            && psr.IsAcceptNewestVersionSwitch == null
                            && psr.CancelledAt != null
                            && psr.LastPaidAt.AddDays(30) > _dateHelper.GetNowByAppTimeZone())
                        .ToListAsync();

                    foreach (var registration in registrationsList)
                    {
                        // Find the config object for the current SubscriptionCycleTypeId
                        var config = podcastSubscriptionConfig
                            .FirstOrDefault(psc => psc["SubscriptionCycleTypeId"] != null && psc["SubscriptionCycleTypeId"].ToObject<int>() == registration.SubscriptionCycleTypeId);

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
                            int incomeTakenDelayDays = config["IncomeTakenDelayDays"]?.ToObject<int>() ?? 0;
                            decimal profitRate = config["ProfitRate"]?.ToObject<decimal>() ?? 0;
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
        public async Task<bool> ValidatePodcastSubscriptionRegistration(int accountId, Guid PodcastSubscriptionRegistrationId)
        {
            var podcastSubscriptionRegistration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                .Where(psr => psr.Id == PodcastSubscriptionRegistrationId && psr.AccountId == accountId)
                .FirstOrDefaultAsync();
            return podcastSubscriptionRegistration != null;
        }
        public async Task<JObject?> ValidatePodcastSubscriptionAccess(int accountId, int PodcastSubscriptionId)
        {
            var podcastShowOrChannelIds = await _podcastSubscriptionGenericRepository.FindAll()
                .Where(ps => ps.Id == PodcastSubscriptionId)
                .Select(ps => new { ps.PodcastShowId, ps.PodcastChannelId })
                .FirstOrDefaultAsync();

            if (podcastShowOrChannelIds == null)
                return null;

            if (podcastShowOrChannelIds.PodcastShowId.HasValue)
                return await GetPodcastShowWithAccountId(accountId, podcastShowOrChannelIds.PodcastShowId.Value);

            if (podcastShowOrChannelIds.PodcastChannelId.HasValue)
                return await GetPodcastChannelWithAccountId(accountId, podcastShowOrChannelIds.PodcastChannelId.Value);

            return null;
        }
        public async Task<JObject?> GetPodcastShowWithAccountId(int accountId, Guid podcastShowId)
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
                                include = "PodcastShowStatusTracking"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastShowOfAccount"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.First as JObject
                : null;
        }
        public async Task<JObject?> GetPodcastChannelWithAccountId(int accountId, Guid podcastChannelId)
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
                                include = "PodcastChannelStatusTracking"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastChannelOfAccount"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                ? podcastChannelArray.First as JObject
                : null;
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
            Console.WriteLine("Real Result: " + realResult);
            return realResult != null ? realResult.ToObject<PodcastShowDTO>() : null;
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
    }
}
