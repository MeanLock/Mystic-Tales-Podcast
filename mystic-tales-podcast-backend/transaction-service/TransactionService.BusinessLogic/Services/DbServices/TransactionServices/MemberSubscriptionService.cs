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
    public class MemberSubscriptionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<MemberSubscriptionService> _logger;
        private readonly IGenericRepository<MemberSubscriptionTransaction> _memberSubscriptionTransactionGenericRepository;
        private readonly IPayosConfig _payosConfig;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly DateHelper _dateHelper;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        public MemberSubscriptionService(
            AppDbContext appDbContext,
            ILogger<MemberSubscriptionService> logger,
            IGenericRepository<MemberSubscriptionTransaction> memberSubscriptionTransactionGenericRepository,
            IPayosConfig payosConfig,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            DateHelper dateHelper,
            HttpServiceQueryClient httpServiceQueryClient)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _memberSubscriptionTransactionGenericRepository = memberSubscriptionTransactionGenericRepository;
            _payosConfig = payosConfig;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _dateHelper = dateHelper;
            _httpServiceQueryClient = httpServiceQueryClient;
        }
        public async Task<List<MemberSubscriptionTransaction>> GetMemberSubscriptionTransactions(Guid memberSubscriptionRegistartionId)
        {
            return await _memberSubscriptionTransactionGenericRepository.FindAll()
                .Include(pst => pst.TransactionType)
                .Include(pst => pst.TransactionStatus)
                .Where(pst => pst.MemberSubscriptionRegistrationId.Equals(memberSubscriptionRegistartionId))
                .ToListAsync();
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
