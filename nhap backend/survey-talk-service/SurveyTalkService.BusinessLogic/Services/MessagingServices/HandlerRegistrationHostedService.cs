using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SurveyTalkService.BusinessLogic.Services.MessagingServices.interfaces;
using SurveyTalkService.Infrastructure.Services.Kafka;

namespace SurveyTalkService.BusinessLogic.Services.MessagingServices
{
    /// <summary>
    /// Background service responsible for registering message handlers and consuming Kafka messages
    /// Separates concerns: KafkaConsumerService provides utilities, this service handles background processing
    /// </summary>
    public class HandlerRegistrationHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHandlerRegistryService _handlerRegistry;
        private readonly ILogger<HandlerRegistrationHostedService> _logger;

        public HandlerRegistrationHostedService(
            IServiceProvider serviceProvider,
            IHandlerRegistryService handlerRegistry,
            ILogger<HandlerRegistrationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _handlerRegistry = handlerRegistry;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HandlerRegistrationHostedService starting...");

            try
            {
                // Small delay to ensure application has started
                await Task.Delay(1000, stoppingToken);

                // Register handlers from HandlerRegistryService
                RegisterAllHandlers();

                // Get KafkaConsumerService from Infrastructure using reflection to avoid circular dependency
                var kafkaConsumerService = GetKafkaConsumerService();
                
                if (kafkaConsumerService != null)
                {
                    // Initialize Kafka consumer
                    await InitializeKafkaConsumer(kafkaConsumerService);

                    // Subscribe to topics
                    SubscribeToTopics(kafkaConsumerService);

                    _logger.LogInformation("HandlerRegistrationHostedService initialized successfully - starting message consumption");

                    // Start consuming messages
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = ConsumeMessage(kafkaConsumerService, stoppingToken);
                            
                            if (consumeResult != null && HasMessage(consumeResult))
                            {
                                await ProcessMessage(kafkaConsumerService, consumeResult);
                                CommitMessage(kafkaConsumerService, consumeResult);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("HandlerRegistrationHostedService consume operation cancelled");
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error consuming message - will retry after delay");
                            await Task.Delay(5000, stoppingToken);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("KafkaConsumerService not found - handlers registered but no message consumption");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("HandlerRegistrationHostedService operation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandlerRegistrationHostedService - will continue without Kafka consumer");
                // Don't rethrow - let application continue without Kafka consumer
            }
            finally
            {
                _logger.LogInformation("HandlerRegistrationHostedService stopped");
            }
        }

        private void RegisterAllHandlers()
        {
            try
            {
                _logger.LogInformation("Registering all message handlers...");
                
                // Register handlers through HandlerRegistryService
                _handlerRegistry.RegisterAllHandlers();
                
                // Get KafkaConsumerService and register handlers with it
                var kafkaConsumerService = GetKafkaConsumerService();
                
                if (kafkaConsumerService != null)
                {
                    var handlers = _handlerRegistry.GetAllHandlers();
                    var topicMessageTypes = _handlerRegistry.GetTopicMessageTypes();
                    
                    // Register each handler with the consumer service
                    foreach (var handler in handlers)
                    {
                        // Find which topic this message type belongs to
                        var topic = topicMessageTypes.FirstOrDefault(t => t.Value.Contains(handler.Key)).Key;
                        
                        if (!string.IsNullOrEmpty(topic))
                        {
                            RegisterMessageTypeHandler(kafkaConsumerService, handler.Key, topic, handler.Value);
                        }
                        else
                        {
                            _logger.LogWarning("No topic found for message type: {MessageType}", handler.Key);
                        }
                    }
                    
                    _logger.LogInformation("Registered {HandlerCount} handlers from {TopicCount} topics with KafkaConsumerService", 
                        handlers.Count, topicMessageTypes.Count);
                }
                
                _logger.LogInformation("Handler registration completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register handlers");
                throw;
            }
        }

        private KafkaConsumerService? GetKafkaConsumerService()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var kafkaConsumerType = scope.ServiceProvider.GetRequiredService<KafkaConsumerService>();
                if (kafkaConsumerType != null)
                {
                    return kafkaConsumerType;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting KafkaConsumerService");
                return null;
            }
        }

        private async Task InitializeKafkaConsumer(KafkaConsumerService kafkaConsumerService)
        {
            try
            {
                await kafkaConsumerService.InitializeConsumerAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Kafka consumer");
                throw;
            }
        }

        private void SubscribeToTopics(KafkaConsumerService kafkaConsumerService)
        {
            try
            {
                kafkaConsumerService.SubscribeToTopics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topics");
            }
        }

        private void RegisterMessageTypeHandler(KafkaConsumerService kafkaConsumerService, string messageType, string topic, Func<string, string, Task> handler)
        {
            try
            {
                kafkaConsumerService.RegisterMessageTypeHandler(messageType, topic, handler);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering message type handler for {MessageType}", messageType);
            }
        }

        private ConsumeResult<string, string>? ConsumeMessage(KafkaConsumerService kafkaConsumerService, CancellationToken cancellationToken)
        {
            try
            {
                return kafkaConsumerService.ConsumeMessage(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming message");
                return null;
            }
        }

        private async Task ProcessMessage(KafkaConsumerService kafkaConsumerService, ConsumeResult<string, string> consumeResult)
        {
            try
            {
                await kafkaConsumerService.ProcessMessageAsync(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                throw;
            }

        }

        private void CommitMessage(KafkaConsumerService kafkaConsumerService, ConsumeResult<string, string> consumeResult)
        {
            try
            {
                // var commitMethod = kafkaConsumerService.GetType().GetMethod("CommitMessage");
                // commitMethod?.Invoke(kafkaConsumerService, new object[] { consumeResult });
                kafkaConsumerService.CommitMessage(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing message");
            }
        }

        private bool HasMessage(ConsumeResult<string, string> consumeResult)
        {
            try
            {
                var messageProperty = consumeResult.GetType().GetProperty("Message");
                var message = messageProperty?.GetValue(consumeResult);
                return message != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if consume result has message");
                return false;
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation("HandlerRegistrationHostedService disposing...");
            base.Dispose();
        }
    }
}
