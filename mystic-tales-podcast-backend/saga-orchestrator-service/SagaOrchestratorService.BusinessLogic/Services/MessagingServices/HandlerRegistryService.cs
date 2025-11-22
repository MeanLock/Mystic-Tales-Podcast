using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SagaOrchestratorService.BusinessLogic.Attributes;
using SagaOrchestratorService.BusinessLogic.Services.MessagingServices.interfaces;
using SagaOrchestratorService.BusinessLogic.MessageHandlers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using SagaOrchestratorService.Common.AppConfigurations.Saga.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.Saga;

namespace SagaOrchestratorService.BusinessLogic.Services.MessagingServices
{
    public class HandlerRegistryService : IHandlerRegistryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HandlerRegistryService> _logger;
        private readonly Dictionary<string, (Type HandlerType, MethodInfo Method)> _handlerMethods;
        private readonly Dictionary<string, Func<string, string, Task>> _messageHandlers;
        private readonly Dictionary<string, List<string>> _runtimeTopicMessageTypes = new();
        private readonly Dictionary<string, List<string>> _runtimeTopicMessageNames = new();

        public HandlerRegistryService(
            IServiceProvider serviceProvider,
            ILogger<HandlerRegistryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _handlerMethods = new Dictionary<string, (Type, MethodInfo)>();
            _messageHandlers = new Dictionary<string, Func<string, string, Task>>();
        }

        public Dictionary<string, Func<string, string, Task>> GetAllHandlers() => _messageHandlers;

        public Dictionary<string, List<string>> GetTopicMessageNames()
        {
            var topicMessageNames = new Dictionary<string, List<string>>();

            var allHandlers = CollectAllHandlers();
            foreach (var handler in allHandlers)
            {
                if (!topicMessageNames.ContainsKey(handler.Topic))
                    topicMessageNames[handler.Topic] = new List<string>();
                if (!topicMessageNames[handler.Topic].Contains(handler.Attribute.MessageName))
                    topicMessageNames[handler.Topic].Add(handler.Attribute.MessageName);
            }

            foreach (var kvp in _runtimeTopicMessageTypes)
            {
                if (!topicMessageNames.TryGetValue(kvp.Key, out var list))
                {
                    list = new List<string>();
                    topicMessageNames[kvp.Key] = list;
                }
                foreach (var mt in kvp.Value)
                    if (!list.Contains(mt)) list.Add(mt);
            }

            return topicMessageNames;
        }

        public void RegisterAllHandlers()
        {
            var allHandlers = CollectAllHandlers();
            foreach (var handlerInfo in allHandlers)
                RegisterSingleHandler(handlerInfo.HandlerType, handlerInfo.Method, handlerInfo.Attribute);

            _logger.LogInformation("Completed handler registration from assembly. Total handlers: {Count}", _messageHandlers.Count);
        }

        private List<(Type HandlerType, MethodInfo Method, MessageHandlerAttribute Attribute, string Topic)> CollectAllHandlers()
        {
            var result = new List<(Type, MethodInfo, MessageHandlerAttribute, string)>();

            // Resolve YAML-backed flow configuration
            using var scope = _serviceProvider.CreateScope();
            var flowConfig = scope.ServiceProvider.GetRequiredService<ISagaFlowConfig>();

            if (!flowConfig.Loaded || flowConfig.Flows.Count == 0)
            {
                _logger.LogInformation("SagaFlowConfig not loaded or empty. No saga handlers to collect.");
                return result;
            }

            // Flow start handler: FlowMessageHandler.HandleFlowAsync
            var flowHandlerType = typeof(SagaOrchestratorService.BusinessLogic.MessageHandlers.FlowMessageHandler);
            var flowMethod = flowHandlerType.GetMethod("HandleFlowAsync", BindingFlags.Instance | BindingFlags.Public)
                            ?? throw new InvalidOperationException("HandleFlowAsync not found on FlowMessageHandler");

            // Emit handler: FlowStepEmitMessageHandler.HandleEmitAsync
            var emitHandlerType = typeof(SagaOrchestratorService.BusinessLogic.MessageHandlers.FlowStepEmitMessageHandler);
            var emitMethod = emitHandlerType.GetMethod("HandleEmitAsync", BindingFlags.Instance | BindingFlags.Public)
                            ?? throw new InvalidOperationException("HandleEmitAsync not found on FlowStepEmitMessageHandler");

            // Map flows (messageType = flow name, topic = flow.Topic)
            foreach (var (flowName, def) in flowConfig.Flows)
            {
                if (string.IsNullOrWhiteSpace(def.Topic)) continue;

                var virtualAttr = new MessageHandlerAttribute(flowName, def.Topic);
                result.Add((flowHandlerType, flowMethod, virtualAttr, def.Topic));

                // Map emits under each step (messageType = emit, topic = outcome.Topic)
                foreach (var step in def.Steps)
                {
                    if (step.OnSuccess != null && !string.IsNullOrWhiteSpace(step.OnSuccess.Emit) && !string.IsNullOrWhiteSpace(step.OnSuccess.Topic))
                    {
                        var successAttr = new MessageHandlerAttribute(step.OnSuccess.Emit, step.OnSuccess.Topic);
                        result.Add((emitHandlerType, emitMethod, successAttr, step.OnSuccess.Topic));
                        
                        // Add debugging for the specific problematic message
                        //if (step.OnSuccess.Emit.Contains("create-podcast-subscription-transaction"))
                        //{
                        //    _logger.LogError("DEBUG: Registering SUCCESS emit handler - MessageName: {MessageName}, Topic: {Topic}, Flow: {Flow}, Step: {Step}", 
                        //        step.OnSuccess.Emit, step.OnSuccess.Topic, flowName, step.Name);
                        //}
                    }

                    if (step.OnFailure != null && !string.IsNullOrWhiteSpace(step.OnFailure.Emit) && !string.IsNullOrWhiteSpace(step.OnFailure.Topic))
                    {
                        var failureAttr = new MessageHandlerAttribute(step.OnFailure.Emit, step.OnFailure.Topic);
                        result.Add((emitHandlerType, emitMethod, failureAttr, step.OnFailure.Topic));
                        
                        // Add debugging for the specific problematic message
                        //if (step.OnFailure.Emit.Contains("create-podcast-subscription-transaction"))
                        //{
                        //    _logger.LogError("DEBUG: Registering FAILURE emit handler - MessageName: {MessageName}, Topic: {Topic}, Flow: {Flow}, Step: {Step}", 
                        //        step.OnFailure.Emit, step.OnFailure.Topic, flowName, step.Name);
                        //}
                    }
                }
            }

            _logger.LogInformation("DEBUG: Total handlers collected: {Count}", result.Count);
            return result;
        }

        private void RegisterSingleHandler(Type handlerType, MethodInfo method, MessageHandlerAttribute attribute)
        {
            var handlerKey = $"{attribute.MessageName}_{attribute.Topic}";
            _handlerMethods[handlerKey] = (handlerType, method);

            var wrapperDelegate = CreateHandlerWrapper(handlerType, method);
            _messageHandlers[attribute.MessageName] = wrapperDelegate;

            _logger.LogInformation(
                "Registered handler: {HandlerType}.{MethodName} for MessageName: {MessageName} on Topic: {Topic}",
                handlerType.Name, method.Name, attribute.MessageName, attribute.Topic);
        }

        private Func<string, string, Task> CreateHandlerWrapper(Type handlerType, MethodInfo method)
        {
            return async (key, messageJson) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handlerInstance = scope.ServiceProvider.GetRequiredService(handlerType);
                var task = (Task)method.Invoke(handlerInstance, new object[] { key, messageJson })!;
                await task;
            };
        }

        public void UnregisterHandler(string messageName)
        {
            _logger.LogInformation("Unregistered handler for MessageName: {MessageName}", messageName);
        }
    }
}
