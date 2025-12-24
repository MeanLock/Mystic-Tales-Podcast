using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.MemberSubscription.Details;
using SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateMemberSubscription;
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
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices
{
    public class MemberSubscriptionService
    {
        private readonly IGenericRepository<MemberSubscription> _memberSubscriptionGenericRepository;
        private readonly IGenericRepository<MemberSubscriptionCycleTypePrice> _memberSubscriptionCycleTypePriceGenericRepository;
        private readonly IGenericRepository<MemberSubscriptionBenefitMapping> _memberSubscriptionBenefitMappingGenericRepository;
        private readonly IGenericRepository<MemberSubscriptionRegistration> _memberSubscriptionRegistrationGenericRepository;

        private readonly ILogger<MemberSubscriptionService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public MemberSubscriptionService(
            IGenericRepository<MemberSubscription> memberSubscriptionGenericRepository,
            IGenericRepository<MemberSubscriptionCycleTypePrice> memberSubscriptionCycleTypePriceGenericRepository,
            IGenericRepository<MemberSubscriptionBenefitMapping> memberSubscriptionBenefitMappingGenericRepository,
            IGenericRepository<MemberSubscriptionRegistration> memberSubscriptionRegistrationGenericRepository,
            ILogger<MemberSubscriptionService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            GenericQueryService genericQueryService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper)
        {
            _memberSubscriptionGenericRepository = memberSubscriptionGenericRepository;
            _memberSubscriptionCycleTypePriceGenericRepository = memberSubscriptionCycleTypePriceGenericRepository;
            _memberSubscriptionBenefitMappingGenericRepository = memberSubscriptionBenefitMappingGenericRepository;
            _memberSubscriptionRegistrationGenericRepository = memberSubscriptionRegistrationGenericRepository;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<List<MemberSubscription>?> GetAllMemberSubscriptionsAsync(int roleId)
        {
            var query = _memberSubscriptionGenericRepository.FindAll();
            if(roleId == 1) // assuming 1 is admin role
            {
                query = query.Where(ms => ms.IsActive);
            }
            return await query.ToListAsync();
        }
        public async Task CreateMemberSubscriptionAsync(CreateMemberSubscriptionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var memberSubscription = null as MemberSubscription;
                    var cycleTypePrices = null as List<MemberSubscriptionCycleTypePrice>;
                    var benefitMappings = null as List<MemberSubscriptionBenefitMapping>;

                    var newMemberSubscription = new MemberSubscription
                    {
                        Name = parameter.Name,
                        Description = parameter.Description,
                        CurrentVersion = 1,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                        UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    memberSubscription = await _memberSubscriptionGenericRepository.CreateAsync(newMemberSubscription);
                    foreach (var cycleTypePrice in parameter.MemberSubscriptionCycleTypePriceList)
                    {
                        var newCycleTypePrice = new MemberSubscriptionCycleTypePrice
                        {
                            MemberSubscriptionId = memberSubscription.Id,
                            SubscriptionCycleTypeId = cycleTypePrice.SubscriptionCycleTypeId,
                            Version = 1,
                            Price = cycleTypePrice.Price,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var cycleTypePriceResult = await _memberSubscriptionCycleTypePriceGenericRepository.CreateAsync(newCycleTypePrice);
                        if (cycleTypePrices == null)
                        {
                            cycleTypePrices.Add(cycleTypePriceResult);
                        }
                    }
                    foreach (var benefitId in parameter.MemberSubscriptionBenefitMappingList)
                    {
                        var newBenefitMapping = new MemberSubscriptionBenefitMapping
                        {
                            MemberSubscriptionId = memberSubscription.Id,
                            MemberSubscriptionBenefitId = benefitId,
                            Version = 1,
                            CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                            UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                        };
                        var benefitMappingResult = await _memberSubscriptionBenefitMappingGenericRepository.CreateAsync(newBenefitMapping);
                        if (benefitMappings == null)
                        {
                            benefitMappings.Add(benefitMappingResult);
                        }
                    }

                    await transaction.CommitAsync();
                    var newResponseData = new JObject
                    {
                        { "MemberSubscriptionId", memberSubscription.Id },
                        { "Name", memberSubscription.Name },
                        { "Description", memberSubscription.Description },
                        { "MemberSubscriptionCycleTypePriceList", JArray.FromObject(cycleTypePrices) },
                        { "MemberSubscriptionBenefitMappingList", JArray.FromObject(benefitMappings) },
                        { "CreatedAt", memberSubscription.CreatedAt }
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
                    _logger.LogInformation("Successfully created member subscription for SagaId: {SagaId}", sagaId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating member subscription for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Create member subscription failed, error: " + ex.Message }
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
                    _logger.LogInformation("Create member subscription failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        //Not Implemented yet
        public async Task<MemberSubscriptionDetailResponseDTO> GetMemberSubscriptionIdAsync(int memberSubscriptionId)
        {
            //var memberSubscription = await _memberSubscriptionGenericRepository.FindAll(
            //    includeFunc: ps => ps
            //        .Include(sct => sct.MemberSubscriptionCycleTypePrices)
            //        .ThenInclude(sct => sct.SubscriptionCycleType)
            //        .Include(bm => bm.MemberSubscriptionBenefitMappings)
            //        .ThenInclude(bm => bm.MemberSubscriptionBenefit)
            //        .Include(sr => sr.MemberSubscriptionRegistrations)
            //        .ThenInclude(sr => sr.SubscriptionCycleType)
            //    )
            //    .Where(ps => ps.Id == memberSubscriptionId)
            //    .Select(ps => new PodcastSubscriptionDetailResponseDTO
            //    {
            //        Id = ps.Id,
            //        Name = ps.Name,
            //        Description = ps.Description,
            //        PodcastShowId = ps.PodcastShowId,
            //        PodcastChannelId = ps.PodcastChannelId,
            //        IsActive = ps.DeletedAt == null,
            //        CurrentVersion = ps.CurrentVersion,
            //        DeletedAt = ps.DeletedAt,
            //        CreatedAt = ps.CreatedAt,
            //        UpdatedAt = ps.UpdatedAt,
            //        PodcastSubscriptionCycleTypePriceList = ps.PodcastSubscriptionCycleTypePrices
            //            .Select(ctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
            //            {
            //                PodcastSubscriptionId = ctp.PodcastSubscriptionId,
            //                SubscriptionCycleType = ctp.SubscriptionCycleType == null
            //                ? null
            //                : new SubscriptionCycleTypeDTO
            //                {
            //                    Id = ctp.SubscriptionCycleType.Id,
            //                    Name = ctp.SubscriptionCycleType.Name
            //                },
            //                Version = ctp.Version,
            //                Price = ctp.Price,
            //                CreatedAt = ctp.CreatedAt,
            //                UpdatedAt = ctp.UpdatedAt
            //            }).ToList(),
            //        PodcastSubscriptionBenefitMappingList = ps.PodcastSubscriptionBenefitMappings
            //            .Select(bm => new DTOs.PodcastSubscription.ListItems.PodcastSubscriptionBenefitMappingListItemResponseDTO
            //            {
            //                PodcastSubscriptionId = bm.PodcastSubscriptionId,
            //                PodcastSubscriptionBenefit = bm.PodcastSubscriptionBenefit == null
            //                ? null
            //                : new PodcastSubscriptionBenefitDTO
            //                {
            //                    Id = bm.PodcastSubscriptionBenefit.Id,
            //                    Name = bm.PodcastSubscriptionBenefit.Name
            //                },
            //                Version = bm.Version,
            //                CreatedAt = bm.CreatedAt,
            //                UpdatedAt = bm.UpdatedAt
            //            }).ToList(),
            //        PodcastSubscriptionRegistrationList = ps.PodcastSubscriptionRegistrations
            //            .Select(sr => new DTOs.PodcastSubscription.ListItems.PodcastSubscriptionRegistrationListItemResponseDTO
            //            {
            //                Id = sr.Id,
            //                AccountId = sr.AccountId ?? 0,
            //                PodcastSubscriptionId = sr.PodcastSubscriptionId,
            //                SubscriptionCycleType = sr.SubscriptionCycleType == null
            //                ? null
            //                : new SubscriptionCycleTypeDTO
            //                {
            //                    Id = sr.SubscriptionCycleType.Id,
            //                    Name = sr.SubscriptionCycleType.Name
            //                },
            //                CurrentVersion = sr.CurrentVersion,
            //                IsAcceptNewestVersionSwitch = sr.IsAcceptNewestVersionSwitch,
            //                IsIncomeTaken = sr.IsIncomeTaken,
            //                LastPaidAt = sr.LastPaidAt,
            //                CancelledAt = sr.CancelledAt,
            //                CreatedAt = sr.CreatedAt,
            //                UpdatedAt = sr.UpdatedAt
            //            }).ToList()
            //    })
            //    .FirstOrDefaultAsync();
            //if (memberSubscription == null)
            //{
            //    _logger.LogWarning("No Member subscription with ID {MemberSubscriptionId} found.", memberSubscriptionId);
            //    return null;
            //}
            //return memberSubscription;
            return null;
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
    }
}
