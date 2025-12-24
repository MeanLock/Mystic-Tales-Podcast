// MessageHandlers/BaseSagaCommandMessageHandler.cs
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.Infrastructure.Models.Kafka;
using SubscriptionService.Infrastructure.Services.Kafka;

namespace SubscriptionService.BusinessLogic.MessageHandlers
{
    public abstract class BaseSagaCommandMessageHandler : BaseMessageHandler
    {
        protected readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;

        protected BaseSagaCommandMessageHandler(
            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,
            ILogger logger) : base(logger)
        {
            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;
        }

        /// <summary>
        /// Execute saga command with automatic exception handling.
        /// Only emits FAILED message on exception. Success emission is business logic's responsibility.
        /// </summary>
        /// <param name="messageJson">Raw Kafka message JSON</param>
        /// <param name="stepHandler">Business logic handler (should emit success itself)</param>
        /// <param name="responseTopic">Topic to send failed response (default: saga-orchestrator-events)</param>
        /// <param name="failedEmitMessage">Failed event name (e.g., "create-booking.failed")</param>
        protected async Task ExecuteSagaCommandMessageAsync(
            string messageJson,
            Func<SagaCommandMessage, Task> stepHandler,
            string? responseTopic = null,
            string? failedEmitMessage = null)
        {
            SagaCommandMessage? command = null;

            try
            {
                // Deserialize command message
                var sagaCommandMessage = DeserializeMessage<SagaCommandMessage>(messageJson);
                command = sagaCommandMessage;

                if (command == null)
                {
                    _logger.LogWarning("SagaCommandMessage is null, cannot process step");
                    return;
                }

                _logger.LogInformation(
                    "Executing saga step. SagaId: {SagaId}, MessageName: {MessageName}, FlowName: {FlowName}",
                    command.SagaInstanceId,
                    command.MessageName,
                    command.FlowName);

                // Execute business logic (business logic will emit success itself)
                await stepHandler(command);

                _logger.LogInformation(
                    "Saga step completed. SagaId: {SagaId}, MessageName: {MessageName}",
                    command.SagaInstanceId,
                    command.MessageName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Saga step failed with unhandled exception. SagaId: {SagaId}, MessageName: {MessageName}",
                    command?.SagaInstanceId,
                    command?.MessageName);

                if (command != null)
                {
                    // Auto-generate failed emit name if not provided
                    var resolvedFailedEmitMessage = failedEmitMessage ?? $"{command.MessageName}.failed";
                    var resolvedTopic = responseTopic ?? "saga-orchestrator-events";

                    // Emit failed event
                    await EmitFailedSagaEventAsync(
                        command: command,
                        errorMessage: ex.Message,
                        eventName: resolvedFailedEmitMessage,
                        topic: resolvedTopic
                    );
                }
                else
                {
                    _logger.LogError(
                        "Cannot emit failed message: command deserialization failed");
                }
            }
        }

        /// <summary>
        /// Emit failed saga event back to orchestrator
        /// </summary>
        private async Task EmitFailedSagaEventAsync(
            SagaCommandMessage command,
            string errorMessage,
            string eventName,
            string topic)
        {
            // Response data chỉ chứa ErrorMessage
            var responseData = new JObject
            {
                ["ErrorMessage"] = errorMessage
            };

            _logger.LogDebug(
                "Emitting failed saga event. SagaId: {SagaId}, Event: {EventName}, Error: {Error}",
                command.SagaInstanceId,
                eventName,
                errorMessage);

            var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(topic, command.RequestData, responseData, command.SagaInstanceId, command.FlowName, eventName);

            // Send saga event using SendSagaMessageAsync
            var success = await _messagingService.SendSagaMessageAsync(
                sagaEventMessage           // Failed emit name
            );

            if (success)
            {
                _logger.LogWarning(
                    "Emitted failed saga event. SagaId: {SagaId}, EventName: {EventName}, Topic: {Topic}",
                    command.SagaInstanceId,
                    eventName,
                    topic);
            }
            else
            {
                _logger.LogError(
                    "Failed to emit failed saga event! SagaId: {SagaId}, EventName: {EventName}, Topic: {Topic}",
                    command.SagaInstanceId,
                    eventName,
                    topic);
            }
        }
    }
}