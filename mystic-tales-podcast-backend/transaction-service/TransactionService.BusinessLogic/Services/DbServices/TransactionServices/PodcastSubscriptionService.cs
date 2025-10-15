using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreatePodcastSubscriptionTransaction;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Helpers.DateHelpers;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities.SqlServer;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Infrastructure.Models.Kafka;
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
        public async Task<List<PodcastSubscriptionTransaction>> GetPodcastSubscriptionTransactions(Guid podcastSubscriptionRegistartionId)
        {
            return await _podcastSubscriptionTransactionGenericRepository.FindAll()
                .Include(pst => pst.TransactionType)
                .Include(pst => pst.TransactionStatus)
                .Where(pst => pst.PodcastSubscriptionRegistrationId.Equals(podcastSubscriptionRegistartionId))
                .ToListAsync();
        }
        public async Task CreatePodcastSubscriptionTransactionAsync(CreatePodcastSubscriptionTransactionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var transactionTypeId = parameter.TransactionTypeId;
                    var newPodcastSubscriptionTransaction = null as PodcastSubscriptionTransaction;
                    switch (transactionTypeId)
                    {
                        case 8:
                            var cyclePaymentPodcastSubscriptionTransaction = new PodcastSubscriptionTransaction
                            {
                                PodcastSubscriptionRegistrationId = parameter.PodcastSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                Profit = null,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.CreateAsync(cyclePaymentPodcastSubscriptionTransaction);
                            break;
                        case 9:
                            var cyclePaymentRefundPodcastSubscriptionTransaction = new PodcastSubscriptionTransaction
                            {
                                PodcastSubscriptionRegistrationId = parameter.PodcastSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                Profit = null,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.CreateAsync(cyclePaymentRefundPodcastSubscriptionTransaction);
                            break;
                        case 10:
                            var systemSubscriptionPodcastSubscriptionTransaction = new PodcastSubscriptionTransaction
                            {
                                PodcastSubscriptionRegistrationId = parameter.PodcastSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                Profit = null,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.CreateAsync(systemSubscriptionPodcastSubscriptionTransaction);
                            break;
                        case 11:
                            break;
                        default:
                            throw new Exception("Invalid TransactionTypeId for podcast subscription transaction: " + transactionTypeId);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating podcast subscription transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Creating podcast subscription transaction failed, error: " + ex.Message }
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
                    _logger.LogInformation("Creating podcast subscription transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CompletePodcastSubscriptionTransactionAsync(CompletePodcastSubscriptionTransactionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while completing podcast subscription transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Completing podcast subscription transaction failed, error: " + ex.Message }
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
                    _logger.LogInformation("Completing podcast subscription transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
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
