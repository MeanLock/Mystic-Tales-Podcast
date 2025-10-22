using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Attributes;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.Infrastructure.Services.Kafka;

namespace TransactionService.BusinessLogic.Services.MessagingServices
{
    public class HandlerRegistryService : IHandlerRegistryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HandlerRegistryService> _logger;
        private readonly Dictionary<string, (Type HandlerType, MethodInfo Method)> _handlerMethods;
        private readonly Dictionary<string, Func<string, string, Task>> _messageHandlers;

        public HandlerRegistryService(
            IServiceProvider serviceProvider,
            ILogger<HandlerRegistryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _handlerMethods = new Dictionary<string, (Type, MethodInfo)>();
            _messageHandlers = new Dictionary<string, Func<string, string, Task>>();
        }

        public Dictionary<string, Func<string, string, Task>> GetAllHandlers()
        {
            return _messageHandlers;
        }

        public Dictionary<string, List<string>> GetTopicMessageTypes()
        {
            var topicMessageTypes = new Dictionary<string, List<string>>();
            
            var allHandlers = CollectAllHandlers();
            foreach (var handler in allHandlers)
            {
                if (!topicMessageTypes.ContainsKey(handler.Topic))
                {
                    topicMessageTypes[handler.Topic] = new List<string>();
                }
                
                if (!topicMessageTypes[handler.Topic].Contains(handler.Attribute.MessageType))
                {
                    topicMessageTypes[handler.Topic].Add(handler.Attribute.MessageType);
                }
            }
            
            return topicMessageTypes;
        }

        public void RegisterAllHandlers()
        {
            // Thu thập tất cả handlers
            var allHandlers = CollectAllHandlers();
            
            // Register handlers - lưu trữ trong dictionary để KafkaConsumerService có thể lấy
            foreach (var handlerInfo in allHandlers)
            {
                RegisterSingleHandler(handlerInfo.HandlerType, handlerInfo.Method, handlerInfo.Attribute);
            }
            
            _logger.LogInformation("Completed handler registration from assembly. Total handlers: {Count}", _messageHandlers.Count);
        }

        private List<(Type HandlerType, MethodInfo Method, MessageHandlerAttribute Attribute, string Topic)> CollectAllHandlers()
        {
            var result = new List<(Type, MethodInfo, MessageHandlerAttribute, string)>();
            
            var assembly = Assembly.GetExecutingAssembly();
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("MessageHandler"))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                Console.WriteLine($"Registering handler: {handlerType.Name}");
                
                var methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<MessageHandlerAttribute>() != null)
                    .ToList();

                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<MessageHandlerAttribute>()!;
                    result.Add((handlerType, method, attribute, attribute.Topic));
                }
            }
            
            return result;
        }

        private void RegisterSingleHandler(Type handlerType, MethodInfo method, MessageHandlerAttribute attribute)
        {
            // Lưu trữ handler type và method info thay vì tạo instance ngay
            var handlerKey = $"{attribute.MessageType}_{attribute.Topic}";
            _handlerMethods[handlerKey] = (handlerType, method);

            // Tạo wrapper delegate để resolve handler khi runtime
            var wrapperDelegate = CreateHandlerWrapper(handlerType, method);

            // Lưu trữ handler delegate để KafkaConsumerService có thể lấy
            _messageHandlers[attribute.MessageType] = wrapperDelegate;

            _logger.LogInformation(
                "Registered handler: {HandlerType}.{MethodName} for MessageType: {MessageType} on Topic: {Topic}",
                handlerType.Name, method.Name, attribute.MessageType, attribute.Topic);
        }

        private Func<string, string, Task> CreateHandlerWrapper(Type handlerType, MethodInfo method)
        {
            return async (key, messageJson) =>
            {
                // Tạo scope mới để resolve scoped services
                using var scope = _serviceProvider.CreateScope();
                var handlerInstance = scope.ServiceProvider.GetRequiredService(handlerType);
                
                // Invoke method với scoped handler instance
                var task = (Task)method.Invoke(handlerInstance, new object[] { key, messageJson })!;
                await task;
            };
        }

        public void UnregisterHandler(string messageType)
        {
            _logger.LogInformation("Unregistered handler for MessageType: {MessageType}", messageType);
        }
    }
}
