using SagaOrchestratorService.BusinessLogic.Services.MessagingServices.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.Saga.interfaces;
using SagaOrchestratorService.Infrastructure.Models.Kafka;
using Microsoft.Extensions.Logging;
using SagaOrchestratorService.DataAccess.Enums.Saga;
using SagaOrchestratorService.BusinessLogic.Services.DbServices;
using SagaOrchestratorService.BusinessLogic.Services.DbServices.SagaServices;
using SagaOrchestratorService.Infrastructure.Services.Kafka;

namespace SagaOrchestratorService.BusinessLogic.MessageHandlers
{
    public class FlowStepEmitMessageHandler : BaseMessageHandler
    {
        private readonly IMessagingService _messaging;
        private readonly ISagaFlowConfig _flowConfig;
        private readonly SagaInstanceService _sagaInstanceService;
        private readonly KafkaProducerService _kafkaProducerService;

        public FlowStepEmitMessageHandler(
            IMessagingService messaging,
            ISagaFlowConfig flowConfig,
            SagaInstanceService sagaInstanceService,
            KafkaProducerService kafkaProducerService,
            ILogger<FlowStepEmitMessageHandler> logger) : base(logger)
        {
            _messaging = messaging;
            _flowConfig = flowConfig;
            _sagaInstanceService = sagaInstanceService;
            _kafkaProducerService = kafkaProducerService;
        }

        // Invoked via registry wrapper (same signature as Facility handlers)
        public async Task HandleEmitAsync(string key, string messageJson)
        {
            try
            {
                var message = DeserializeMessage<SagaEventMessage>(messageJson);
                if (message == null)
                {
                    _logger.LogWarning("FlowStepEmitMessageHandler: Failed to deserialize message");
                    return;
                }

                var emit = message.MessageName;
                var sagaId = message.SagaInstanceId;
                var flowName = message.FlowName;
                var requestData = message.RequestData;
                var responseData = message.LastStepResponseData;

                if (string.IsNullOrWhiteSpace(emit))
                {
                    _logger.LogWarning("FlowStepEmitMessageHandler: missing emit/MessageName in payload");
                    return;
                }

                if (!_flowConfig.Loaded || _flowConfig.Flows.Count == 0)
                {
                    _logger.LogWarning("FlowStepEmitMessageHandler: flow config not loaded");
                    return;
                }

                var outcome = await FindOutcomeByEmit(emit, sagaId);
                if (outcome == null)
                {
                    _logger.LogWarning("Something went really wrong");
                    return;
                }

                var currentSagaId = sagaId;

                // Determine if this is success or failure based on emit name
                var isSuccess = emit.EndsWith(".success", StringComparison.OrdinalIgnoreCase);
                var isFailure = emit.EndsWith(".failed", StringComparison.OrdinalIgnoreCase) || emit.EndsWith(".failure", StringComparison.OrdinalIgnoreCase);

                // Extract step name from emit (e.g., "create-order.success" -> "create-order")
                var stepName = emit.Contains('.') ? emit.Substring(0, emit.LastIndexOf('.')) : emit;

                // Extract error message from RequestData if present
                var errorMessage = responseData["ErrorMessage"]?.ToString();

                if (isSuccess)
                {
                    // Update step execution status to SUCCESS with request and response data
                    await _sagaInstanceService.UpdateStepExecutionStatusAsync(currentSagaId, stepName, SagaStepStatusEnum.SUCCESS, SerializeToJson(requestData), SerializeToJson(responseData), null);

                    // Keep saga status as RUNNING - do not update ResultData yet
                    await _sagaInstanceService.UpdateSagaStatusAsync(currentSagaId, SagaFlowStatusEnum.RUNNING, null, null, null);

                    // Check if no more next steps - check for saga completion
                    if (outcome.Value.NextSteps.Count == 0)
                    {
                        await _sagaInstanceService.UpdateSagaCurrentStepAsync(currentSagaId, null);

                        // Check if saga is complete and update ResultData only when completing
                        var isCompleted = await _sagaInstanceService.CheckAndUpdateSagaCompletionAsync(currentSagaId);
                        if (isCompleted)
                        {
                            var resultDataJson = SerializeToJson(responseData);
                            // Update ResultData only when saga completes successfully
                            await _sagaInstanceService.UpdateSagaStatusAsync(currentSagaId, SagaFlowStatusEnum.SUCCESS, resultDataJson, null, null);
                            _logger.LogInformation("Saga completed successfully: {SagaId}, ResultData updated", currentSagaId);
                        }
                    }
                    else
                    {
                        // Fan-out next steps
                        foreach (var step in outcome.Value.NextSteps)
                        {
                            // Create step execution for next step
                            await _sagaInstanceService.CreateStepExecutionAsync(currentSagaId, step.Name, step.Topic, SerializeToJson(requestData));

                            // Update saga current step
                            await _sagaInstanceService.UpdateSagaCurrentStepAsync(currentSagaId, step.Name);

                            var SagaCommandMessage = _kafkaProducerService.PrepareSagaCommandMessage(step.Topic, requestData, responseData, currentSagaId, flowName, step.Name);
                            var result = await _messaging.SendSagaMessageAsync(SagaCommandMessage);
                            if (result)
                                _logger.LogInformation("Emit '{Emit}' -> step '{Step}' sent to '{Topic}' (SagaId: {SagaId})",
                                emit, step.Name, step.Topic, currentSagaId);
                            else
                            {
                                _logger.LogWarning("Emit '{Emit}' -> failed to send step '{Step}' to '{Topic}' (SagaId: {SagaId})",
                                emit, step.Name, step.Topic, currentSagaId);
                            }
                        }
                    }



                    // 2) Start next flows (multiple flows support)
                    foreach (var nextFlow in outcome.Value.NextFlows)
                    {
                        if (string.IsNullOrWhiteSpace(nextFlow.Name))
                            continue;

                        if (!_flowConfig.Flows.TryGetValue(nextFlow.Name, out var flowDef) || string.IsNullOrWhiteSpace(flowDef.Topic))
                        {
                            _logger.LogWarning("Emit '{Emit}' -> unknown next flow '{Flow}'", emit, nextFlow.Name);
                        }
                        else
                        {
                            // Use the topic from nextFlow if specified, otherwise fallback to flowDef.Topic
                            var targetTopic = !string.IsNullOrWhiteSpace(nextFlow.Topic) ? nextFlow.Topic : flowDef.Topic;
                            
                            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(targetTopic, requestData, null, nextFlow.Name);
                            var result = await _messaging.SendSagaMessageAsync(startSagaTriggerMessage);
                            if(result)
                                _logger.LogInformation("Emit '{Emit}' -> start flow '{Flow}' to '{Topic}'",
                                    emit, nextFlow.Name, targetTopic);
                            else
                            {
                                _logger.LogWarning("Emit '{Emit}' -> failed to start flow '{Flow}' to '{Topic}'", emit, nextFlow.Name, targetTopic);
                            }
                        }
                    }
                }
                else if (isFailure)
                {
                    // Update step execution status to FAILED with request and response data and error message
                    var stepErrorMessage = errorMessage ?? $"Step failed with emit: {emit}";
                    await _sagaInstanceService.UpdateStepExecutionStatusAsync(currentSagaId, stepName, SagaStepStatusEnum.FAILED, SerializeToJson(requestData), SerializeToJson(responseData), stepErrorMessage);

                    // Update saga status to FAILED with error message AND ResultData
                    var sagaErrorMessage = errorMessage ?? $"Saga failed at step: {stepName}";
                    var resultDataJson = SerializeToJson(responseData);
                    await _sagaInstanceService.UpdateSagaStatusAsync(currentSagaId, SagaFlowStatusEnum.FAILED, resultDataJson, stepName, sagaErrorMessage);

                    foreach (var step in outcome.Value.NextSteps)
                    {
                        // Create step execution for next step
                        await _sagaInstanceService.CreateStepExecutionAsync(currentSagaId, step.Name, step.Topic, SerializeToJson(requestData));

                        // Update saga current step
                        await _sagaInstanceService.UpdateSagaCurrentStepAsync(currentSagaId, step.Name);

                        var SagaCommandMessage = _kafkaProducerService.PrepareSagaCommandMessage(step.Topic, requestData, responseData, currentSagaId, flowName, step.Name);
                        var result = await _messaging.SendSagaMessageAsync(SagaCommandMessage);
                        if (result)
                            _logger.LogInformation("Emit '{Emit}' -> step '{Step}' sent to '{Topic}' (SagaId: {SagaId})",
                            emit, step.Name, step.Topic, currentSagaId);
                        else
                        {
                            _logger.LogWarning("Emit '{Emit}' -> failed to send step '{Step}' to '{Topic}' (SagaId: {SagaId})",
                            emit, step.Name, step.Topic, currentSagaId);
                        }
                    }

                    foreach (var nextFlow in outcome.Value.NextFlows)
                    {
                        if (string.IsNullOrWhiteSpace(nextFlow.Name))
                            continue;

                        if (!_flowConfig.Flows.TryGetValue(nextFlow.Name, out var flowDef) || string.IsNullOrWhiteSpace(flowDef.Topic))
                        {
                            _logger.LogWarning("Emit '{Emit}' -> unknown next flow '{Flow}'", emit, nextFlow.Name);
                        }
                        else
                        {
                            // Use the topic from nextFlow if specified, otherwise fallback to flowDef.Topic
                            var targetTopic = !string.IsNullOrWhiteSpace(nextFlow.Topic) ? nextFlow.Topic : flowDef.Topic;

                            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(targetTopic, requestData, null, nextFlow.Name);
                            var result = await _messaging.SendSagaMessageAsync(startSagaTriggerMessage);
                            if (result)
                                _logger.LogInformation("Emit '{Emit}' -> start flow '{Flow}' to '{Topic}'",
                                    emit, nextFlow.Name, targetTopic);
                            else
                            {
                                _logger.LogWarning("Emit '{Emit}' -> failed to start flow '{Flow}' to '{Topic}'", emit, nextFlow.Name, targetTopic);
                            }
                        }
                    }

                    _logger.LogWarning("Saga failed: {SagaId}, Step: {StepName}, Emit: {Emit}, Error: {ErrorMessage}, ResultData updated",
                        currentSagaId, stepName, emit, sagaErrorMessage);
                }
                else
                {
                    _logger.LogWarning("Unknown emit type: {Emit}. Cannot determine success or failure.", emit);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FlowStepEmitMessageHandler failed");
                throw;
            }
        }

        private async Task<(List<(string Name, string Topic)> NextSteps, List<(string Name, string Topic)> NextFlows)?> FindOutcomeByEmit(string emit, Guid sagaId)
        {
            // If no flowName provided, fall back to searching all flows (backward compatibility)
            var sagaInstance = await _sagaInstanceService.GetSagaInstanceAsync(sagaId);

            var flowName = sagaInstance?.FlowName;

            if (string.IsNullOrWhiteSpace(flowName))
            {
                return FindOutcomeByEmitInAllFlows(emit);
            }

            // Search only in the specified flow
            if (!_flowConfig.Flows.TryGetValue(flowName, out var flow))
            {
                _logger.LogWarning("Flow '{FlowName}' not found in configuration", flowName);
                return null;
            }

            foreach (var step in flow.Steps)
            {
                if (step.OnSuccess != null && string.Equals(step.OnSuccess.Emit, emit, StringComparison.OrdinalIgnoreCase))
                {
                    var steps = (step.OnSuccess.NextSteps ?? new()).Select(s => (s.Name, s.Topic)).ToList();
                    var flows = (step.OnSuccess.NextFlows ?? new()).Select(f => (f.Name, f.Topic)).ToList();
                    return (steps, flows);
                }
                if (step.OnFailure != null && string.Equals(step.OnFailure.Emit, emit, StringComparison.OrdinalIgnoreCase))
                {
                    var steps = (step.OnFailure.NextSteps ?? new()).Select(s => (s.Name, s.Topic)).ToList();
                    var flows = (step.OnFailure.NextFlows ?? new()).Select(f => (f.Name, f.Topic)).ToList();
                    return (steps, flows);
                }
            }
            _logger.LogWarning("Emit '{Emit}' not found in flow '{FlowName}'", emit, flowName);
            return null;
        }

        private (List<(string Name, string Topic)> NextSteps, List<(string Name, string Topic)> NextFlows)? FindOutcomeByEmitInAllFlows(string emit)
        {
            foreach (var (_, flow) in _flowConfig.Flows)
            {
                foreach (var step in flow.Steps)
                {
                    if (step.OnSuccess != null && string.Equals(step.OnSuccess.Emit, emit, StringComparison.OrdinalIgnoreCase))
                    {
                        var steps = (step.OnSuccess.NextSteps ?? new()).Select(s => (s.Name, s.Topic)).ToList();
                        var flows = (step.OnSuccess.NextFlows ?? new()).Select(f => (f.Name, f.Topic)).ToList();
                        return (steps, flows);
                    }
                    if (step.OnFailure != null && string.Equals(step.OnFailure.Emit, emit, StringComparison.OrdinalIgnoreCase))
                    {
                        var steps = (step.OnFailure.NextSteps ?? new()).Select(s => (s.Name, s.Topic)).ToList();
                        var flows = (step.OnFailure.NextFlows ?? new()).Select(f => (f.Name, f.Topic)).ToList();
                        return (steps, flows);
                    }
                }
            }
            return null;
        }
    }
}