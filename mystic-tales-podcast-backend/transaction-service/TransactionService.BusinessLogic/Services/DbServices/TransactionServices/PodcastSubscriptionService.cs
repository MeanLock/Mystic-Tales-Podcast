using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompletePodcastSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreatePodcastSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreatePodcastSubscriptionTransactionRollback;
using TransactionService.BusinessLogic.DTOs.Podcast;
using TransactionService.BusinessLogic.DTOs.PodcastSubscription;
using TransactionService.BusinessLogic.DTOs.SystemConfiguration;
using TransactionService.BusinessLogic.Enums.Kafka;
using TransactionService.BusinessLogic.Enums.Transaction;
using TransactionService.BusinessLogic.Helpers.DateHelpers;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Entities;
using TransactionService.DataAccess.Entities.SqlServer;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Infrastructure.Models.Kafka;
using TransactionService.Infrastructure.Services.Kafka;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                .Select(pst => new PodcastSubscriptionTransaction
                {
                    Id = pst.Id,
                    PodcastSubscriptionRegistrationId = pst.PodcastSubscriptionRegistrationId,
                    Amount = pst.Amount,
                    TransactionType = pst.TransactionType,
                    TransactionStatus = pst.TransactionStatus,
                    CreatedAt = pst.CreatedAt,
                    UpdatedAt = pst.UpdatedAt
                })
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

                    var systemConfig = await GetActiveSystemConfigProfile();

                    var transactionTypeId = parameter.TransactionTypeId;
                    var newPodcastSubscriptionTransaction = null as PodcastSubscriptionTransaction;
                    switch (transactionTypeId)
                    {
                        case (int)TransactionTypeEnum.CustomerSubscriptionCyclePayment:
                            var cyclePaymentPodcastSubscriptionTransaction = new PodcastSubscriptionTransaction
                            {
                                PodcastSubscriptionRegistrationId = parameter.PodcastSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.CreateAsync(cyclePaymentPodcastSubscriptionTransaction);
                            break;
                        case (int)TransactionTypeEnum.CustomerSubscriptionCyclePaymentRefund:
                            var cyclePaymentRefundPodcastSubscriptionTransaction = new PodcastSubscriptionTransaction
                            {
                                PodcastSubscriptionRegistrationId = parameter.PodcastSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.CreateAsync(cyclePaymentRefundPodcastSubscriptionTransaction);
                            break;
                        case (int)TransactionTypeEnum.SystemSubscriptionIncome:
                            var systemIncomePodcastSubscriptionTransaction = new PodcastSubscriptionTransaction
                            {
                                PodcastSubscriptionRegistrationId = parameter.PodcastSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Success,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.CreateAsync(systemIncomePodcastSubscriptionTransaction);
                            break;
                        case (int)TransactionTypeEnum.PodcasterSubscriptionIncome:
                            var podcasterIncomePodcastSubscriptionTransaction = new PodcastSubscriptionTransaction
                            {
                                PodcastSubscriptionRegistrationId = parameter.PodcastSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                Profit = parameter.Profit,
                                TransactionTypeId = parameter.TransactionTypeId,
                                TransactionStatusId = (int)TransactionStatusEnum.Pending,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.CreateAsync(podcasterIncomePodcastSubscriptionTransaction);
                            var requestData = new JObject
                            {
                                { "PodcastSubscriptionRegistrationId", newPodcastSubscriptionTransaction.PodcastSubscriptionRegistrationId },
                                { "Profit", null },
                                { "AccountId", parameter.AccountId },
                                { "PodcasterId", parameter.PodcasterId },
                                { "Amount", newPodcastSubscriptionTransaction.Profit },
                                { "TransactionTypeId", 10 }
                            };

                            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("payment-processing-domain", requestData, null, "podcast-subscription-system-payment-flow");
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
                            _logger.LogInformation($"Send start saga trigger message for SagaId: {startSagaTriggerMessage.SagaInstanceId} to flow podcast-subscription-system-payment-flow Successfully");
                            break;
                        default:
                            throw new Exception("Invalid TransactionTypeId for podcast subscription transaction: " + transactionTypeId);
                    }

                    await transaction.CommitAsync();

                    var newRequestData = command.RequestData;
                    newRequestData["PodcastSubscriptionTransactionId"] = newPodcastSubscriptionTransaction.Id;

                    var newResponseData = new JObject{
                        { "PodcastSubscriptionTransactionId", newPodcastSubscriptionTransaction.Id },
                        { "CreatedAt", newPodcastSubscriptionTransaction.CreatedAt}
                    };
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Create podcast subscription transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);

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
                    _logger.LogError("Creating podcast subscription transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
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

                    var podcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.FindByIdAsync(parameter.PodcastSubscriptionTransactionId);
                    if (podcastSubscriptionTransaction == null)
                    {
                        throw new Exception($"No podcast subscription transaction found for Id: {parameter.PodcastSubscriptionTransactionId}");
                    }
                    if (podcastSubscriptionTransaction.TransactionStatusId != (int)TransactionStatusEnum.Pending)
                    {
                        throw new Exception($"This podcast subscription transaction is not eligible for completion");
                    }
                    podcastSubscriptionTransaction.TransactionStatusId = (int)TransactionStatusEnum.Success;
                    var newPodcastSubscriptionTransaction = await _podcastSubscriptionTransactionGenericRepository.UpdateAsync(podcastSubscriptionTransaction.Id, podcastSubscriptionTransaction);

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    newResponseData["UpdatedAt"] = newPodcastSubscriptionTransaction.UpdatedAt;
                    var newMessageName = messageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: sagaId,
                        flowName: flowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, sagaId.ToString());
                    _logger.LogInformation("Complete podcast subscription transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
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
        public async Task CreatePodcastSubscriptionTransactionRollbackAsync(CreatePodcastSubscriptionTransactionRollbackParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    PodcastSubscriptionTransaction? podcastSubscriptionTransaction = _podcastSubscriptionTransactionGenericRepository.FindByIdAsync(parameter.PodcastSubscriptionTransactionId).Result;
                    if (podcastSubscriptionTransaction == null)
                    {
                        throw new Exception("Podcast Subscription Transaction not found.");
                    }
                    ;
                    podcastSubscriptionTransaction.TransactionStatusId = (int)TransactionStatusEnum.Error; // Thay đổi trạng thái giao dịch thành "Thất bại"
                    await _podcastSubscriptionTransactionGenericRepository.UpdateAsync(podcastSubscriptionTransaction.Id, podcastSubscriptionTransaction);
                    await transaction.CommitAsync();
                    var newResponseData = new JObject{
                        { "PodcastSubscriptionTransactionId", podcastSubscriptionTransaction.Id },
                        { "TransactionStatusId", podcastSubscriptionTransaction.TransactionStatusId}
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
                    _logger.LogInformation("Rollback create podcast subscription transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while rolling back create podcast subscription transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Rollback create podcast subscription transactionnt failed, error: " + ex.Message }
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
                    _logger.LogInformation("Rollback create podcast subscription transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
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
                                    Id = podcastSubscriptionRegistartionId,
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
        public async Task<List<PodcastSubscriptionDTO>?> GetPodcastSubscriptionByShowId(Guid podcastShowId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastSubscription",
                            QueryType = "findall",
                            EntityType = "PodcastSubscription",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    PodcastShowId = podcastShowId
                                },
                                include = "PodcastSubscriptionRegistrations"
                            })
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", batchRequest);

            return result.Results?["podcastSubscription"] is JArray podcastShowArray && podcastShowArray.Count > 0
                ? podcastShowArray.ToObject<List<PodcastSubscriptionDTO>>()
                : null;
            //Console.WriteLine("Real Result: " + realResult);
        }
    }
}
