// MessageHandlers/BaseSagaStepHandler.cs
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.Infrastructure.Models.Kafka;

namespace UserService.BusinessLogic.MessageHandlers
{
    public abstract class BaseSagaCommandMessageHandler : BaseMessageHandler
    {
        protected readonly IMessagingService _messagingService;

        protected BaseSagaCommandMessageHandler(
            IMessagingService messagingService,
            ILogger<BaseSagaCommandMessageHandler> logger) : base(logger)
        {
            _messagingService = messagingService;
        }

        /// <summary>
        /// Execute saga step with automatic exception handling
        /// </summary>
        /// <param name="messageJson">Raw message JSON</param>
        /// <param name="stepHandler">Business logic handler</param>
        /// <param name="responseTopic">Topic to send response (default: saga orchestrator topic)</param>
        /// <param name="successEmit">Success event name from YAML (e.g., "create-booking.success")</param>
        /// <param name="failedEmit">Failed event name from YAML (e.g., "create-booking.failed")</param>
        protected async Task ExecuteSagaCommandMessageAsync(
            string messageJson,
            Func<SagaCommandMessage, Task<JObject>> stepHandler,
            string? responseTopic = null,
            string? successEmit = null,
            string? failedEmit = null)
        {
            SagaCommandMessage command = null;

            try
            {
                // Deserialize command message
                var envelope = DeserializeMessage<MessageEnvelope<SagaCommandMessage>>(messageJson);
                command = envelope?.Data;

                if (command == null)
                {
                    _logger.LogWarning("SagaCommandMessage is null");
                    return;
                }

                _logger.LogInformation(
                    "Executing saga step. SagaId: {SagaId}, MessageName: {MessageName}, FlowName: {FlowName}",
                    command.SagaId,
                    command.MessageName,
                    command.FlowName);

                // Execute business logic
                var responseData = await stepHandler(command);

                // Auto-generate emit name if not provided
                var resolvedSuccessEmit = successEmit ?? $"{command.MessageName}.success";
                var resolvedTopic = responseTopic ?? "saga-orchestrator-events";

                // Emit success event
                await EmitSagaEventAsync(
                    command: command,
                    responseData: responseData,
                    eventName: resolvedSuccessEmit,
                    topic: resolvedTopic,
                    isSuccess: true,
                    errorMessage: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Saga step failed. SagaId: {SagaId}, MessageName: {MessageName}",
                    command?.SagaId,
                    command?.MessageName);

                if (command != null)
                {
                    // Auto-generate emit name if not provided
                    var resolvedFailedEmit = failedEmit ?? $"{command.MessageName}.failed";
                    var resolvedTopic = responseTopic ?? "saga-orchestrator-events";

                    // Emit failed event
                    await EmitSagaEventAsync(
                        command: command,
                        responseData: null,
                        eventName: resolvedFailedEmit,
                        topic: resolvedTopic,
                        isSuccess: false,
                        errorMessage: FormatErrorMessage(ex)
                    );
                }
            }
        }

        /// <summary>
        /// Emit saga event back to orchestrator
        /// </summary>
        private async Task EmitSagaEventAsync(
            SagaCommandMessage command,
            JObject? responseData,
            string eventName,
            string topic,
            bool isSuccess,
            string? errorMessage)
        {
            // Merge request data with response data
            var mergedData = new JObject(command.RequestData);
            if (command.LastStepResponseData != null && command.LastStepResponseData.HasValues)
            {
                mergedData.Merge(command.LastStepResponseData);
            }

            // Prepare response data (null if failed)
            var finalResponseData = responseData ?? new JObject();
            if (!isSuccess && !string.IsNullOrEmpty(errorMessage))
            {
                finalResponseData["error"] = errorMessage;
            }

            // Send saga event using SendSagaMessageAsync
            var success = await _messagingService.SendSagaMessageAsync<SagaEventMessage>(
                topic: topic,
                key: command.SagaId.ToString(),
                requestData: mergedData,
                responseData: finalResponseData,
                sagaId: command.SagaId,
                flowName: command.FlowName,
                messageName: eventName
            );

            if (success)
            {
                _logger.LogInformation(
                    "Emitted saga event. SagaId: {SagaId}, EventName: {EventName}, IsSuccess: {IsSuccess}",
                    command.SagaId,
                    eventName,
                    isSuccess);
            }
            else
            {
                _logger.LogError(
                    "Failed to emit saga event. SagaId: {SagaId}, EventName: {EventName}",
                    command.SagaId,
                    eventName);
            }
        }

        private string FormatErrorMessage(Exception exception)
        {
            var errorDetails = new
            {
                Type = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = _logger.IsEnabled(LogLevel.Debug) ? exception.StackTrace : null,
                InnerException = exception.InnerException?.Message
            };

            return System.Text.Json.JsonSerializer.Serialize(errorDetails);
        }
    }
}
