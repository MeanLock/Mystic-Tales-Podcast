using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CompleteMemberSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateMemberSubscriptionTransaction;
using TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateMemberSubscriptionTransactionRollback;
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
                .Select(pst => new MemberSubscriptionTransaction
                {
                    Id = pst.Id,
                    MemberSubscriptionRegistrationId = pst.MemberSubscriptionRegistrationId,
                    Amount = pst.Amount,
                    TransactionType = pst.TransactionType,
                    TransactionStatus = pst.TransactionStatus,
                    CreatedAt = pst.CreatedAt,
                    UpdatedAt = pst.UpdatedAt
                })
                .ToListAsync();
        }
        public async Task CreateMemberSubscriptionTransactionAsync(CreateMemberSubscriptionTransactionParameterDTO parameter, SagaCommandMessage command)
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
                    var newMemberSubscriptionTransaction = null as MemberSubscriptionTransaction;
                    switch (transactionTypeId)
                    {
                        case 8:
                            var cyclePaymentMemberSubscriptionTransaction = new MemberSubscriptionTransaction
                            {
                                MemberSubscriptionRegistrationId = parameter.MemberSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                TransactionTypeId = transactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newMemberSubscriptionTransaction = await _memberSubscriptionTransactionGenericRepository.CreateAsync(cyclePaymentMemberSubscriptionTransaction);
                            var requestData = new JObject
                            {
                                { "MemberSubscriptionRegistrationId", newMemberSubscriptionTransaction.MemberSubscriptionRegistrationId },
                                { "AccountId", parameter.AccountId },
                                { "Amount", parameter.Amount },
                                { "TransactionTypeId", 10 }
                            };

                            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("payment-processing-domain", requestData, null, "member-subscription-system-payment-flow");
                            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
                            _logger.LogInformation($"Send start saga trigger message for SagaId: {startSagaTriggerMessage.SagaInstanceId} to flow member-subscription-system-payment-flow Successfully");
                            break;
                        case 9:
                            var cyclePaymentRefundMemberSubscriptionTransaction = new MemberSubscriptionTransaction
                            {
                                MemberSubscriptionRegistrationId = parameter.MemberSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                TransactionTypeId = transactionTypeId,
                                TransactionStatusId = 1,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newMemberSubscriptionTransaction = await _memberSubscriptionTransactionGenericRepository.CreateAsync(cyclePaymentRefundMemberSubscriptionTransaction);
                            break;
                        case 10:
                            var systemIncomeMemberSubscriptionTransaction = new MemberSubscriptionTransaction
                            {
                                MemberSubscriptionRegistrationId = parameter.MemberSubscriptionRegistrationId,
                                Amount = parameter.Amount,
                                TransactionTypeId = transactionTypeId,
                                TransactionStatusId = 2,
                                CreatedAt = _dateHelper.GetNowByAppTimeZone(),
                                UpdatedAt = _dateHelper.GetNowByAppTimeZone()
                            };
                            newMemberSubscriptionTransaction = await _memberSubscriptionTransactionGenericRepository.CreateAsync(systemIncomeMemberSubscriptionTransaction);
                            break;
                        default:
                            throw new Exception("Invalid TransactionTypeId for podcast subscription transaction: " + transactionTypeId);
                    }

                    await transaction.CommitAsync();

                    var newRequestData = command.RequestData;
                    newRequestData["MemberSubscriptionTransactionId"] = newMemberSubscriptionTransaction.Id;

                    var newResponseData = new JObject
                    {
                        { "MemberSubscriptionTransactionId", newMemberSubscriptionTransaction.Id },
                        { "CreatedAt", newMemberSubscriptionTransaction.CreatedAt }
                    };
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: newRequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Created member subscription transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while creating member subscription transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Creating member subscription transaction failed, error: " + ex.Message }
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
                    _logger.LogInformation("Creating member subscription transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CompleteMemberSubscriptionTransactionAsync(CompleteMemberSubscriptionTransactionParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    var memberSubscriptionTransaction = await _memberSubscriptionTransactionGenericRepository.FindByIdAsync(parameter.MemberSubscriptionTransactionId);
                    if (memberSubscriptionTransaction != null)
                    {
                        throw new Exception($"No podcast subscription transaction found for Id: {parameter.MemberSubscriptionTransactionId}");
                    }
                    if (memberSubscriptionTransaction.TransactionStatusId != 1)
                    {
                        throw new Exception($"This podcast subscription transaction is not eligible for completion");
                    }
                    memberSubscriptionTransaction.TransactionStatusId = 2;
                    var newPodcastSubscriptionTransaction = await _memberSubscriptionTransactionGenericRepository.UpdateAsync(memberSubscriptionTransaction.Id, memberSubscriptionTransaction);

                    await transaction.CommitAsync();
                    var newResponseData = command.RequestData;
                    newResponseData["UpdateAt"] = newPodcastSubscriptionTransaction.UpdatedAt;
                    var newMessageName = command.MessageName + ".success";
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PaymentProcessingDomain,
                        requestData: command.RequestData,
                        responseData: newResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: newMessageName);
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage, command.SagaInstanceId.ToString());
                    _logger.LogInformation("Completed member subscription transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while completing member subscription transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Completing member subscription transaction failed, error: " + ex.Message }
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
                    _logger.LogInformation("Completing member subscription transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
        }
        public async Task CreateMemberSubscriptionTransactionRollbackAsync(CreateMemberSubscriptionTransactionRollbackParameterDTO parameter, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var messageName = command.MessageName;
                    var sagaId = command.SagaInstanceId;
                    var flowName = command.FlowName;
                    var responseData = command.LastStepResponseData;

                    MemberSubscriptionTransaction? memberSubscriptionTransaction = _memberSubscriptionTransactionGenericRepository.FindByIdAsync(parameter.MemberSubscriptionTransactionId).Result;
                    if (memberSubscriptionTransaction == null)
                    {
                        throw new Exception("Member Subscription Transaction not found.");
                    }
                    ;
                    memberSubscriptionTransaction.TransactionStatusId = 4; // Thay đổi trạng thái giao dịch thành "Thất bại"
                    await _memberSubscriptionTransactionGenericRepository.UpdateAsync(memberSubscriptionTransaction.Id, memberSubscriptionTransaction);
                    await transaction.CommitAsync();
                    var newResponseData = new JObject{
                        { "MemberSubscriptionTransactionId", memberSubscriptionTransaction.Id },
                        { "TransactionStatusId", memberSubscriptionTransaction.TransactionStatusId}
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
                    _logger.LogInformation("Rollback create member subscription transaction successfully for SagaId: {SagaId}", command.SagaInstanceId);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while rolling back create member subscription transaction for SagaId: {SagaId}", command.SagaInstanceId);
                    var newResponseData = new JObject{
                        { "ErrorMessage", "Rollback create member subscription transactionnt failed, error: " + ex.Message }
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
                    _logger.LogInformation("Rollback create member subscription transaction failed for SagaId: {SagaId}", command.SagaInstanceId);
                }
            }
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
