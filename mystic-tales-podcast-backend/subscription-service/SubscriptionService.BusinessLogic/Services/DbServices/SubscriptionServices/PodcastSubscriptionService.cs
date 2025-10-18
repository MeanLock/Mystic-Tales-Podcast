using Amazon.Runtime.Internal.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.ActivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateAccountPodcastSubscriptionRegistration;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeactivatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeletePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.UpdatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.Details;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems;
using SubscriptionService.BusinessLogic.DTOs.Subscription;
using SubscriptionService.BusinessLogic.Enums.Kafka;
using SubscriptionService.BusinessLogic.Helpers.DateHelpers;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities.SqlServer;
using SubscriptionService.DataAccess.Repositories.interfaces;
using SubscriptionService.Infrastructure.Models.Kafka;
using SubscriptionService.Infrastructure.Services.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

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
        private readonly GenericQueryService _genericQueryService;
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
            GenericQueryService genericQueryService,
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
            _genericQueryService = genericQueryService;
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



                    var podcastSubscription = null as PodcastSubscription;
                    var cycleTypePrices = null as List<PodcastSubscriptionCycleTypePrice>;
                    var benefitMappings = null as List<PodcastSubscriptionBenefitMapping>;
                    if (parameter.PodcastShowId != null)
                    {
                        var isValid = await GetPodcastShow(parameter.AccountId, parameter.PodcastShowId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastShow with Id: {parameter.PodcastShowId}");
                        }
                        var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .FirstOrDefaultAsync(ps => ps.PodcastShowId == parameter.PodcastShowId && ps.DeletedAt == null);
                        if (existPodcastSubscription != null)
                        {
                            throw new Exception($"An Active Podcast Subscription already exists for PodcastShow Id: {parameter.PodcastShowId}");
                        }
                    }
                    if (parameter.PodcastChannelId != null)
                    {
                        var isValid = await GetPodcastChannel(parameter.AccountId, parameter.PodcastChannelId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to create PodcastSubscription for PodcastChanne; with Id: {parameter.PodcastChannelId}");
                        }
                        var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .FirstOrDefaultAsync(ps => ps.PodcastChannelId == parameter.PodcastChannelId && ps.DeletedAt == null);
                        if (existPodcastSubscription != null)
                        {
                            throw new Exception($"An Active Podcast Subscription already exists for PodcastChannel Id: {parameter.PodcastChannelId}");
                        }
                    }
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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

                    if (existPodcastSubscription.PodcastShowId != null)
                    {
                        var isValid = await GetPodcastShow(parameter.AccountId, existPodcastSubscription.PodcastShowId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to update PodcastSubscription for PodcastShow with Id: {existPodcastSubscription.PodcastShowId}");
                        }
                    }
                    if (existPodcastSubscription.PodcastChannelId != null)
                    {
                        var isValid = await GetPodcastChannel(parameter.AccountId, existPodcastSubscription.PodcastChannelId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to update PodcastSubscription for PodcastChannel with Id: {existPodcastSubscription.PodcastChannelId}");
                        }
                    }

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
                        registration.IsAcceptNewestVersionSwitch = null;
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                    if (existPodcastSubscription == null)
                    {
                        _logger.LogWarning("No Active Podcast Subscription exists for PodcastSubscription Id: {PodcastSubscriptionId}", parameter.PodcastSubscriptionId);
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }
                    if (existPodcastSubscription.PodcastShowId != null)
                    {
                        var isValid = await GetPodcastShow(parameter.AccountId, existPodcastSubscription.PodcastShowId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to delete PodcastSubscription for PodcastShow with Id: {existPodcastSubscription.PodcastShowId}");
                        }
                    }
                    if (existPodcastSubscription.PodcastChannelId != null)
                    {
                        var isValid = await GetPodcastChannel(parameter.AccountId, existPodcastSubscription.PodcastChannelId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to delete PodcastSubscription for PodcastChannel with Id: {existPodcastSubscription.PodcastChannelId}");
                        }
                    }
                    existPodcastSubscription.IsActive = false;
                    existPodcastSubscription.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                    existPodcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _podcastSubscriptionGenericRepository.UpdateAsync(existPodcastSubscription.Id, existPodcastSubscription);

                    if (existPodcastSubscription.PodcastShowId != null)
                    {
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
                                    { "TransactionTypeId", 9 }
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
                    var newResponseData = new JObject
                    {
                        { "PodcastSubscriptionId", existPodcastSubscription.Id },
                        { "DeletedAt", existPodcastSubscription.DeletedAt }
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                    var existRegistration = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .FirstOrDefaultAsync(psr => psr.AccountId == parameter.AccountId
                            && psr.PodcastSubscriptionId == parameter.PodcastSubscriptionId
                            && psr.CancelledAt == null);
                    if (existRegistration != null)
                    {
                        throw new Exception($"An Active Podcast Subscription Registration exists for Account Id: {parameter.AccountId} and PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.IsActive != false && ps.DeletedAt != null);
                    if (podcastSubscription == null)
                    {
                        throw new Exception($"No Inactive Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }
                    var show = new JObject();
                    var channel = new JObject();
                    if (podcastSubscription.PodcastShowId != null)
                    {
                        show = await GetPodcastShow(parameter.AccountId, podcastSubscription.PodcastShowId.Value);
                        if (show == null)
                        {
                            throw new($"The Logged In Account is unauthorized to activate PodcastSubscription for PodcastShow with Id: {podcastSubscription.PodcastShowId}");
                        }
                    }
                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        channel = await GetPodcastChannel(parameter.AccountId, podcastSubscription.PodcastChannelId.Value);
                        if (channel == null)
                        {
                            throw new($"The Logged In Account is unauthorized to activate PodcastSubscription for PodcastChannel with Id: {podcastSubscription.PodcastChannelId}");
                        }
                    }
                    if (show.Count > 0)
                    {
                        var result = await _podcastSubscriptionGenericRepository.FindAll().
                            Where(ps => ps.PodcastChannelId.Equals(show["PodcastChannelId"]) && ps.IsActive == true && ps.DeletedAt == null)
                            .FirstOrDefaultAsync();
                        if(result != null)
                        {
                            throw new Exception($"An Active Channel Podcast Subscription exists for PodcastShow Id: {podcastSubscription.PodcastShowId}");
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        .FirstOrDefaultAsync(ps => ps.Id == parameter.PodcastSubscriptionId && ps.IsActive != true && ps.DeletedAt != null);
                    if (podcastSubscription == null)
                    {
                        throw new Exception($"No Active Podcast Subscription exists for PodcastSubscription Id: {parameter.PodcastSubscriptionId}");
                    }
                    if (podcastSubscription.PodcastShowId != null)
                    {
                        var isValid = await GetPodcastShow(parameter.AccountId, podcastSubscription.PodcastShowId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to deactivate PodcastSubscription for PodcastShow with Id: {podcastSubscription.PodcastShowId}");
                        }
                    }
                    if (podcastSubscription.PodcastChannelId != null)
                    {
                        var isValid = await GetPodcastChannel(parameter.AccountId, podcastSubscription.PodcastChannelId.Value);
                        if (isValid == null)
                        {
                            throw new($"The Logged In Account is unauthorized to deactivate PodcastSubscription for PodcastChannel with Id: {podcastSubscription.PodcastChannelId}");
                        }
                    }
                    podcastSubscription.IsActive = false;
                    podcastSubscription.UpdatedAt = _dateHelper.GetNowByAppTimeZone();
                    await _podcastSubscriptionGenericRepository.UpdateAsync(podcastSubscription.Id, podcastSubscription);

                    var activeRegistrations = await _podcastSubscriptionRegistrationGenericRepository.FindAll()
                        .Where(sr => sr.PodcastSubscriptionId == parameter.PodcastSubscriptionId && sr.CancelledAt == null && sr.IsIncomeTaken == false)
                        .ToListAsync();

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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
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
                return await GetPodcastShow(accountId, podcastShowOrChannelIds.PodcastShowId.Value);

            if (podcastShowOrChannelIds.PodcastChannelId.HasValue)
                return await GetPodcastChannel(accountId, podcastShowOrChannelIds.PodcastChannelId.Value);

            return null;
        }
        public async Task<JObject?> GetPodcastShow(int accountId, Guid podcastShowId)
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
                                }
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastShowOfAccount"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.First as JObject
                : null;
        }
        public async Task<JObject?> GetPodcastChannel(int accountId, Guid podcastChannelId)
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
                                }
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);

            return result.Results?["podcastChannelOfAccount"] is JArray podcastChannelArray && podcastChannelArray.Count > 0
                ? podcastChannelArray.First as JObject
                : null;
        }
    }
}
