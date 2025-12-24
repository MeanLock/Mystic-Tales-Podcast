using Microsoft.Extensions.Logging;
using SagaOrchestratorService.BusinessLogic.Services.DbServices;
using SagaOrchestratorService.BusinessLogic.Services.DbServices.SagaServices;
using SagaOrchestratorService.BusinessLogic.Services.MessagingServices.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.Saga.interfaces;
using SagaOrchestratorService.DataAccess.Enums.Saga;
using SagaOrchestratorService.Infrastructure.Models.Kafka;
using SagaOrchestratorService.Infrastructure.Services.Kafka;
using System.Text.Json;

namespace SagaOrchestratorService.BusinessLogic.MessageHandlers
{
    public class FlowMessageHandler : BaseMessageHandler
    {
        private readonly IMessagingService _messaging;
        private readonly ISagaFlowConfig _flowConfig;
        private readonly SagaInstanceService _sagaInstanceService;
        private readonly KafkaProducerService _kafkaProducerService;

        public FlowMessageHandler(
            IMessagingService messaging,
            ISagaFlowConfig flowConfig,
            SagaInstanceService sagaInstanceService,
            KafkaProducerService kafkaProducerService,
            ILogger<FlowMessageHandler> logger) : base(logger)
        {
            _messaging = messaging;
            _flowConfig = flowConfig;
            _sagaInstanceService = sagaInstanceService;
            _kafkaProducerService = kafkaProducerService;
        }

        // Invoked via registry wrapper (same signature as Facility handlers)
        public async Task HandleFlowAsync(string key, string messageJson)
        {
            try
            {
                var message = DeserializeMessage<StartSagaTriggerMessage>(messageJson);
                if (message == null)
                {
                    _logger.LogWarning("FlowStepEmitMessageHandler: Failed to deserialize message");
                    return;
                }

                var messageName = message.MessageName;
                var sagaId = message.SagaInstanceId == Guid.Empty ? Guid.NewGuid() : message.SagaInstanceId;
                var requestData = message.RequestData;
                if (string.IsNullOrWhiteSpace(messageName))
                {
                    _logger.LogWarning("FlowMessageHandler: missing flowName/MessageType in payload");
                    return;
                }

                if (!_flowConfig.Loaded || !_flowConfig.Flows.TryGetValue(messageName, out var flowDef) || flowDef.Steps.Count == 0)
                {
                    _logger.LogWarning("FlowMessageHandler: unknown or empty flow '{Flow}'", messageName);
                    return;
                }

                var first = flowDef.Steps[0];

                // Serialize initial data as JSON string
                string initialDataJson = SerializeToJson(requestData);

                // Create saga instance and first step execution
                await _sagaInstanceService.CreateSagaInstanceAsync(sagaId, messageName, initialDataJson, first.Name);
                await _sagaInstanceService.CreateStepExecutionAsync(sagaId, first.Name, first.Topic, SerializeToJson(requestData));

                var SagaCommandMessage = _kafkaProducerService.PrepareSagaCommandMessage(first.Topic, requestData, null, sagaId, messageName, first.Name);
                var result = await _messaging.SendSagaMessageAsync(SagaCommandMessage);
                if(result)
                _logger.LogInformation("Flow '{Flow}' started -> first step '{Step}' to '{Topic}' (SagaId: {SagaId})",
                    messageName, first.Name, first.Topic, sagaId);
                else
                {
                    await _sagaInstanceService.UpdateSagaStatusAsync(sagaId, SagaFlowStatusEnum.FAILED, null, null, "Send Flow Message Failed");
                    _logger.LogError("Flow '{Flow}' failed to start -> first step '{Step}' to '{Topic}' (SagaId: {SagaId})",
                        messageName, first.Name, first.Topic, sagaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FlowMessageHandler failed");
                throw;
            }
        }

        private static object? ConvertElement(JsonElement el) => el.ValueKind switch
        {
            JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ConvertElement(p.Value)!),
            JsonValueKind.Array => el.EnumerateArray().Select(ConvertElement).ToList(),
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var l) ? l : el.TryGetDouble(out var d) ? d : (object?)el.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => el.GetRawText()
        };
    }
}