using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.Helpers.DateHelpers;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities.SqlServer;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.BusinessLogic.Services.DbServices.TransactionServices
{
    public class PodcastSubscriptionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<PodcastSubscriptionService> _logger;
        private readonly IGenericRepository<PodcastSubscriptionTransaction> _podcastSubscriptionTransactionGenericRepository;
        private readonly IPayosConfig _payosConfig;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly DateHelper _dateHelper;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        public PodcastSubscriptionService(
            AppDbContext appDbContext,
            ILogger<PodcastSubscriptionService> logger,
            IGenericRepository<PodcastSubscriptionTransaction> podcastSubscriptionTransactionGenericRepository,
            IPayosConfig payosConfig,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            DateHelper dateHelper,
            HttpServiceQueryClient httpServiceQueryClient)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _podcastSubscriptionTransactionGenericRepository = podcastSubscriptionTransactionGenericRepository;
            _payosConfig = payosConfig;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _dateHelper = dateHelper;
            _httpServiceQueryClient = httpServiceQueryClient;
        }
        public async  Task<List<PodcastSubscriptionTransaction>> GetPodcastSubscriptionTransactions(Guid podcastSubscriptionRegistartionId)
        {
            return await _podcastSubscriptionTransactionGenericRepository.FindAll()
                .Include(pst => pst.TransactionType)
                .Include(pst => pst.TransactionStatus)
                .Where(pst => pst.PodcastSubscriptionRegistrationId.Equals(podcastSubscriptionRegistartionId))
                .ToListAsync();
        }
        public async Task<JObject?> GetPodcastSubscriptionRegistration(int accountId, Guid podcastSubscriptionRegistartionId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastSubscriptionRegistrationOfAccount",
                            QueryType = "findall",
                            EntityType = "PodcastSubscriptionRegistration",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcastSubscriptionRegistartionId = podcastSubscriptionRegistartionId,
                                    AccountId = accountId
                                }
                            }),
                            Fields = new[] { "Id", "AccountId", "PodcastSubscriptionId"}
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", batchRequest);

            return result.Results?["podcastSubscriptionRegistrationOfAccount"] is JArray podcastSubsciptionRegistrationArray && podcastSubsciptionRegistrationArray.Count > 0
                ? podcastSubsciptionRegistrationArray.First as JObject
                : null;
        }
        public async Task<JObject?> GetMemberSubscriptionRegistration(int accountId, Guid memberSubscriptionRegistartionId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "memberSubscriptionRegistrationOfAccount",
                            QueryType = "findall",
                            EntityType = "MemberSubscriptionRegistration",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    MemberSubscriptionRegistartionId = memberSubscriptionRegistartionId,
                                    AccountId = accountId
                                }
                            }),
                            Fields = new[] { "Id", "AccountId", "MemberSubscriptionId"}
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", batchRequest);

            return result.Results?["memberSubscriptionRegistrationOfAccount"] is JArray memberSubsciptionRegistrationArray && memberSubsciptionRegistrationArray.Count > 0
                ? memberSubsciptionRegistrationArray.First as JObject
                : null;
        }
    }
}
