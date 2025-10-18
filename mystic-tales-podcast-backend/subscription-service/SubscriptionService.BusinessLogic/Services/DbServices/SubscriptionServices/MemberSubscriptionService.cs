using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.Helpers.DateHelpers;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Entities.SqlServer;
using SubscriptionService.DataAccess.Repositories.interfaces;
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
        public async Task<MemberSubscription?> GetMemberSubscriptionByIdAsync(int memberSubscriptionId)
        {
            return await _memberSubscriptionGenericRepository.GetFirstOrDefaultAsync(
                filter: ms => ms.Id == memberSubscriptionId
            );
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
