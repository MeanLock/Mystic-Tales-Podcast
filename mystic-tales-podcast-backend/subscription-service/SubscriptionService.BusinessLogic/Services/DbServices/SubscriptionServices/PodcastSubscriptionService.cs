using Amazon.Runtime.Internal.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription;
using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems;
using SubscriptionService.BusinessLogic.Enums.Kafka;
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
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices
{
    public class PodcastSubscriptionService
    {
        private readonly IGenericRepository<PodcastSubscription> _podcastSubscriptionGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionCycleTypePrice> _podcastSubscriptionCycleTypePriceGenericRepository;
        private readonly IGenericRepository<PodcastSubscriptionBenefitMapping> _podcastSubscriptionBenefitMappingGenericRepository;
        private readonly ILogger<PodcastSubscriptionService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;
        public PodcastSubscriptionService(
            IGenericRepository<PodcastSubscription> podcastSubscriptionGenericRepository,
            IGenericRepository<PodcastSubscriptionCycleTypePrice> podcastSubscriptionCycleTypePriceGenericRepository,
            IGenericRepository<PodcastSubscriptionBenefitMapping> podcastSubscriptionBenefitMappingGenericRepository,
            ILogger<PodcastSubscriptionService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext)
        {
            _podcastSubscriptionGenericRepository = podcastSubscriptionGenericRepository;
            _podcastSubscriptionCycleTypePriceGenericRepository = podcastSubscriptionCycleTypePriceGenericRepository;
            _podcastSubscriptionBenefitMappingGenericRepository = podcastSubscriptionBenefitMappingGenericRepository;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
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
                        var existPodcastSubscription = await _podcastSubscriptionGenericRepository.FindAll()
                            .FirstOrDefaultAsync(ps => ps.PodcastShowId == parameter.PodcastShowId && ps.DeletedAt == null);
                        if(existPodcastSubscription != null)
                        {
                            throw new Exception($"An Active Podcast Subscription already exists for PodcastShow Id: {parameter.PodcastShowId}");
                        }
                    }
                    if(parameter.PodcastChannelId != null)
                    {
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
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
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
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
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
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
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
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: messageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Successfully created podcast subscription for SagaId: {SagaId}", command.SagaInstanceId);
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
                            }),
                            Fields = new[] { "Id", "PodcasterId", "DeletedAt" }
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
                            }),
                            Fields = new[] { "Id", "PodcasterId", "DeletedAt" }
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
