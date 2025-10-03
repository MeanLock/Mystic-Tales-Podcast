using System;
using System.Linq;
using System.Text.Json;
using SagaOrchestratorService.BusinessLogic.Services.MessagingServices.interfaces;
using SagaOrchestratorService.BusinessLogic.Services.SagaServices.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.Saga.interfaces;
using SagaOrchestratorService.DataAccess.Entities;
using SagaOrchestratorService.Infrastructure.Models.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SagaOrchestratorService.BusinessLogic.MessageHandlers
{
    public class FlowStepEmitMessageHandler : BaseMessageHandler
    {
        private readonly IMessagingService _messaging;
        private readonly ISagaFlowConfig _flowConfig;
        private readonly ISagaService _sagaService;

        public FlowStepEmitMessageHandler(
            IMessagingService messaging,
            ISagaFlowConfig flowConfig,
            ISagaService sagaService,
            ILogger<FlowStepEmitMessageHandler> logger) : base(logger)
        {
            _messaging = messaging;
            _flowConfig = flowConfig;
            _sagaService = sagaService;
        }

        // Invoked via registry wrapper (same signature as Facility handlers)
        public async Task HandleEmitAsync(string key, string messageJson)
        {
            try
            {
                var (emit, sagaId, flowName, data) = ExtractEmit(messageJson);
                if (string.IsNullOrWhiteSpace(emit))
                {
                    _logger.LogWarning("FlowStepEmitMessageHandler: missing emit/MessageType in payload");
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
                    _logger.LogWarning("Emit '{Emit}' not found in the Saga {SagaId} Instance", emit, sagaId);
                    return;
                }

                var currentSagaId = sagaId ?? Guid.NewGuid();

                // Determine if this is success or failure based on emit name
                var isSuccess = emit.EndsWith(".success", StringComparison.OrdinalIgnoreCase);
                var isFailure = emit.EndsWith(".failed", StringComparison.OrdinalIgnoreCase) || emit.EndsWith(".failure", StringComparison.OrdinalIgnoreCase);

                // Extract step name from emit (e.g., "create-order.success" -> "create-order")
                var stepName = emit.Contains('.') ? emit.Substring(0, emit.LastIndexOf('.')) : emit;

                if (isSuccess)
                {
                    // Update step execution status to SUCCESS
                    await _sagaService.UpdateStepExecutionStatusAsync(currentSagaId, stepName, StepStatus.SUCCESS, data);

                    // Update saga result data
                    await _sagaService.UpdateSagaStatusAsync(currentSagaId, SagaStatus.RUNNING, data);

                    // 1) Fan-out next steps
                    foreach (var step in outcome.Value.NextSteps)
                    {
                        // Create step execution for next step
                        await _sagaService.CreateStepExecutionAsync(currentSagaId, step.Name, step.Topic, data);

                        // Update saga current step
                        await _sagaService.UpdateSagaCurrentStepAsync(currentSagaId, step.Name);

                        var cmd = new SagaCommandMessage
                        {
                            SagaId = currentSagaId,
                            FlowName = string.IsNullOrWhiteSpace(flowName) ? "(unknown)" : flowName!,
                            MessageName = step.Name,
                            Data = data,
                            MessageType = step.Name
                        };

                        await _messaging.SendSagaMessageAsync(cmd, step.Topic, key);
                        _logger.LogInformation("Emit '{Emit}' -> step '{Step}' sent to '{Topic}' (SagaId: {SagaId})",
                            emit, step.Name, step.Topic, currentSagaId);
                    }

                    // Check if no more next steps - check for saga completion
                    if (outcome.Value.NextSteps.Count == 0)
                    {
                        await _sagaService.UpdateSagaCurrentStepAsync(currentSagaId, null);
                        await _sagaService.CheckAndUpdateSagaCompletionAsync(currentSagaId);
                    }

                    // 2) Start next flows (multiple flows support)
                    foreach (var nextFlow in outcome.Value.NextFlows)
                    {
                        if (string.IsNullOrWhiteSpace(nextFlow))
                            continue;

                        if (!_flowConfig.Flows.TryGetValue(nextFlow, out var flowDef) || string.IsNullOrWhiteSpace(flowDef.Topic))
                        {
                            _logger.LogWarning("Emit '{Emit}' -> unknown next flow '{Flow}'", emit, nextFlow);
                        }
                        else
                        {

                            var start = new StartSagaTriggerMessage
                            {
                                MessageName = nextFlow,
                                Data = data,
                                MessageType = nextFlow
                            };

                            await _messaging.SendSagaMessageAsync(start, flowDef.Topic, key);
                            _logger.LogInformation("Emit '{Emit}' -> start flow '{Flow}' to '{Topic}'",
                                emit, nextFlow, flowDef.Topic);
                        }
                    }
                }
                else if (isFailure)
                {
                    // Update step execution status to FAILED
                    await _sagaService.UpdateStepExecutionStatusAsync(currentSagaId, stepName, StepStatus.FAILED, data, $"Step failed with emit: {emit}");

                    // Update saga status to FAILED
                    await _sagaService.UpdateSagaStatusAsync(currentSagaId, SagaStatus.FAILED, data, stepName, $"Saga failed at step: {stepName}");

                    _logger.LogWarning("Saga failed: {SagaId}, Step: {StepName}, Emit: {Emit}", currentSagaId, stepName, emit);
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

        private async Task<(List<(string Name, string Topic)> NextSteps, List<string> NextFlows)?> FindOutcomeByEmit(string emit, Guid? sagaId)
        {
            // If no flowName provided, fall back to searching all flows (backward compatibility)
            var sagaInstance = await _sagaService.GetSagaInstanceAsync(sagaId ?? Guid.Empty);

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
                    return (steps, step.OnSuccess.NextFlows ?? new());
                }
                if (step.OnFailure != null && string.Equals(step.OnFailure.Emit, emit, StringComparison.OrdinalIgnoreCase))
                {
                    var steps = (step.OnFailure.NextSteps ?? new()).Select(s => (s.Name, s.Topic)).ToList();
                    return (steps, step.OnFailure.NextFlows ?? new());
                }
            }
            _logger.LogWarning("Emit '{Emit}' not found in flow '{FlowName}'", emit, flowName);
            return null;
        }

        private (List<(string Name, string Topic)> NextSteps, List<string> NextFlows)? FindOutcomeByEmitInAllFlows(string emit)
        {
            foreach (var (_, flow) in _flowConfig.Flows)
            {
                foreach (var step in flow.Steps)
                {
                    if (step.OnSuccess != null && string.Equals(step.OnSuccess.Emit, emit, StringComparison.OrdinalIgnoreCase))
                    {
                        var steps = (step.OnSuccess.NextSteps ?? new()).Select(s => (s.Name, s.Topic)).ToList();
                        return (steps, step.OnSuccess.NextFlows ?? new());
                    }
                    if (step.OnFailure != null && string.Equals(step.OnFailure.Emit, emit, StringComparison.OrdinalIgnoreCase))
                    {
                        var steps = (step.OnFailure.NextSteps ?? new()).Select(s => (s.Name, s.Topic)).ToList();
                        return (steps, step.OnFailure.NextFlows ?? new());
                    }
                }
            }
            return null;
        }

        private static (string emit, Guid? sagaId, string? flowName, Dictionary<string, object> data) ExtractEmit(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var emit = root.TryGetProperty("MessageName", out var mt) ? mt.GetString() ?? "" : "";

                Guid? sagaId = null;
                if (root.TryGetProperty("SagaId", out var sid) && sid.ValueKind == JsonValueKind.String && Guid.TryParse(sid.GetString(), out var g))
                    sagaId = g;

                string? flowName = null;
                if (root.TryGetProperty("FlowName", out var fn) && fn.ValueKind == JsonValueKind.String)
                    flowName = fn.GetString();

                var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (root.TryGetProperty("Data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Object)
                {
                    foreach (var p in dataEl.EnumerateObject())
                        data[p.Name] = ConvertElement(p.Value);
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var p in root.EnumerateObject())
                        if (p.Name is not ("MessageName" or "FlowName" or "SagaId"))
                            data[p.Name] = ConvertElement(p.Value);
                }

                return (emit, sagaId, flowName, data);
            }
            catch
            {
                return ("", null, null, new Dictionary<string, object>());
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