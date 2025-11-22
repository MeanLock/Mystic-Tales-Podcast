using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SagaOrchestratorService.Infrastructure.Configurations.Kafka.interfaces;

namespace SagaOrchestratorService.Infrastructure.Services.Kafka
{
    /// <summary>
    /// Singleton service providing Kafka consumer utilities
    /// Responsibilities: Create consumer, manage subscriptions, provide message processing utilities
    /// </summary>
    public class KafkaConsumerService : IDisposable
    {
        private IConsumer<string, string>? _consumer;
        private readonly IKafkaClusterConfig _kafkaClusterConfig;
        private readonly IKafkaConsumerConfig _kafkaConsumerConfig;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly Dictionary<string, Func<string, string, Task>> _messageNameHandlers;
        private readonly Dictionary<string, List<string>> _topicMessageNames;
        private bool _isInitialized = false;

        public KafkaConsumerService(
            IKafkaClusterConfig kafkaClusterConfig,
            IKafkaConsumerConfig kafkaConsumerConfig,
            ILogger<KafkaConsumerService> logger)
        {
            _kafkaClusterConfig = kafkaClusterConfig;
            _kafkaConsumerConfig = kafkaConsumerConfig;
            _logger = logger;
            _messageNameHandlers = new Dictionary<string, Func<string, string, Task>>();
            _topicMessageNames = new Dictionary<string, List<string>>();

            _logger.LogInformation("KafkaConsumerService initialized as singleton utility service");
        }

        /// <summary>
        /// Initialize the Kafka consumer (called by HandlerRegistrationHostedService)
        /// </summary>
        public Task InitializeConsumerAsync()
        {
            if (_isInitialized)
            {
                _logger.LogWarning("Consumer already initialized");
                return Task.CompletedTask;
            }

            try
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = string.Join(",", _kafkaClusterConfig.BootstrapServers),
                    GroupId = _kafkaConsumerConfig.GroupId,
                    ClientId = _kafkaClusterConfig.ClientId,
                    SecurityProtocol = Enum.Parse<SecurityProtocol>(_kafkaClusterConfig.SecurityProtocol),
                    SaslMechanism = Enum.Parse<SaslMechanism>(_kafkaClusterConfig.SaslMechanism),
                    SaslUsername = _kafkaClusterConfig.SaslUsername,
                    SaslPassword = _kafkaClusterConfig.SaslPassword,
                    AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_kafkaConsumerConfig.AutoOffsetReset),
                    EnableAutoCommit = _kafkaConsumerConfig.EnableAutoCommit,
                    SessionTimeoutMs = _kafkaConsumerConfig.SessionTimeoutMs,
                    HeartbeatIntervalMs = _kafkaConsumerConfig.HeartbeatIntervalMs,
                    MaxPollIntervalMs = _kafkaConsumerConfig.MaxPollIntervalMs
                };

                _consumer = new ConsumerBuilder<string, string>(config)
                    .SetErrorHandler((_, e) =>
                    {
                        // _logger.LogError("Consumer error: {Error}", e.Reason)
                    }
                    )
                    .SetLogHandler((_, log) =>
                    {
                        // _logger.LogInformation("Consumer log: {Message}", log.Message)
                    }
                    )
                    .Build();

                _isInitialized = true;
                _logger.LogInformation("Kafka consumer initialized successfully with servers: {Servers}",
                    string.Join(",", _kafkaClusterConfig.BootstrapServers));

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Kafka consumer");
                throw;
            }
        }

        /// <summary>
        /// Register a message name handler
        /// </summary>
        // public void RegisterMessageNameHandler(string messageName, string topic, Func<string, string, Task> handler)
        // {
        //     _messageNameHandlers[messageName] = handler;

        //     if (!_topicMessageNames.ContainsKey(topic))
        //     {
        //         _topicMessageNames[topic] = new List<string>();
        //     }

        //     if (!_topicMessageNames[topic].Contains(messageName))
        //     {
        //         _topicMessageNames[topic].Add(messageName);
        //     }

        //     _logger.LogInformation("Registered handler for MessageName: {MessageName} on Topic: {Topic}",
        //         messageName, topic);
        // }

        public void RegisterMessageNameHandler(string messageName, string topic, Func<string, string, Task> handler)
        {
            // Tạo composite key: "topic:messageName"
            var compositeKey = $"{topic}:{messageName}";
            _messageNameHandlers[compositeKey] = handler;

            if (!_topicMessageNames.ContainsKey(topic))
            {
                _topicMessageNames[topic] = new List<string>();
            }

            if (!_topicMessageNames[topic].Contains(messageName))
            {
                _topicMessageNames[topic].Add(messageName);
            }

            //if (compositeKey.Contains("create-podcast-subscription-transaction.success"))
            //{
            //    _logger.LogError("DEBUG: Successfully registered handler for {compositeKey} on topic {Topic}",
            //        compositeKey, topic);
            //}
            //_logger.LogInformation("-------------------------------------------");
            //_logger.LogInformation("Registered handler for MessageName: {MessageName} on Topic: {Topic}",
            //    messageName, topic);
            //if (compositeKey.Equals("payment-processing-domain:create-booking-system-transaction.success"))
            //    _logger.LogInformation("Registered handler for CREATE PODCAST SUBSCRIPTION TRANSACTION SUCCESS");
        }

        /// <summary>
        /// Subscribe to topics
        /// </summary>
        public void SubscribeToTopics()
        {
            if (_consumer != null && _topicMessageNames.Any())
            {
                var allTopics = _topicMessageNames.Keys.ToList();
                _consumer.Subscribe(allTopics);
                _logger.LogInformation("Subscribed to topics: {Topics}", string.Join(", ", allTopics));
            }
            else
            {
                _logger.LogInformation("No topics to subscribe or consumer not initialized");
            }
        }

        /// <summary>
        /// Consume a single message (called by HandlerRegistrationHostedService)
        /// </summary>
        public ConsumeResult<string, string>? ConsumeMessage(CancellationToken cancellationToken)
        {
            if (_consumer == null)
            {
                _logger.LogWarning("Consumer not initialized");
                return null;
            }

            try
            {
                return _consumer.Consume(cancellationToken);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Consume error: {Error}", ex.Error.Reason);
                return null;
            }
        }

        /// <summary>
        /// Process a consumed message
        /// </summary>
        // public async Task ProcessMessageAsync(ConsumeResult<string, string> result)
        // {
        //     try
        //     {
        //         var messageName = ExtractMessageNameFromHeader(result.Message.Headers)
        //                          ?? ExtractMessageNameFromBody(result.Message.Value);

        //         if (!string.IsNullOrEmpty(messageName) && _messageNameHandlers.ContainsKey(messageName))
        //         {
        //             _logger.LogInformation("Processing message - Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, MessageName: {MessageName}",
        //                 result.Topic, result.Partition.Value, result.Offset.Value, messageName);

        //             await _messageNameHandlers[messageName](result.Message.Key, result.Message.Value);

        //             _logger.LogInformation("Message processed successfully - MessageName: {MessageName}", messageName);
        //         }
        //         else
        //         {
        //             _logger.LogWarning("No handler found for MessageName: {MessageName} from Topic: {Topic}",
        //                 messageName ?? "Unknown", result.Topic);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error processing message from Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
        //             result.Topic, result.Partition.Value, result.Offset.Value);
        //         throw;
        //     }
        // }

        public async Task ProcessMessageAsync(ConsumeResult<string, string> result)
        {
            try
            {
                var messageName = ExtractMessageNameFromHeader(result.Message.Headers)
                                 ?? ExtractMessageNameFromBody(result.Message.Value);

                if (!string.IsNullOrEmpty(messageName))
                {
                    // Thử composite key trước
                    var compositeKey = $"{result.Topic}:{messageName}";

                    if (_messageNameHandlers.ContainsKey(compositeKey))
                    {
                        _logger.LogInformation("Processing message - Topic: {Topic}, MessageName: {MessageName}",
                            result.Topic, messageName);

                        await _messageNameHandlers[compositeKey](result.Message.Key, result.Message.Value);
                    }
                    // Fallback về messageName đơn thuần (backward compatibility)
                    else if (_messageNameHandlers.ContainsKey(messageName))
                    {
                        _logger.LogWarning("Using fallback handler for MessageName: {MessageName}", messageName);
                        await _messageNameHandlers[messageName](result.Message.Key, result.Message.Value);
                    }
                    else
                    {
                        _logger.LogWarning("No handler found for MessageName: {MessageName} from Topic: {Topic}",
                            messageName, result.Topic);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from Topic: {Topic}",
                    result.Topic);
                throw;
            }
        }

        /// <summary>
        /// Commit message offset
        /// </summary>
        public void CommitMessage(ConsumeResult<string, string> result)
        {
            try
            {
                _consumer?.Commit(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing message offset");
            }
        }

        /// <summary>
        /// Get registered handlers for external access
        /// </summary>
        public IReadOnlyDictionary<string, Func<string, string, Task>> GetMessageNameHandlers()
        {
            return _messageNameHandlers.AsReadOnly();
        }

        /// <summary>
        /// Get topic-message name mappings for external access
        /// </summary>
        public IReadOnlyDictionary<string, List<string>> GetTopicMessageNames()
        {
            return _topicMessageNames.ToDictionary(x => x.Key, x => x.Value.ToList()).AsReadOnly();
        }

        private string? ExtractMessageNameFromHeader(Headers headers)
        {
            if (headers != null && headers.TryGetLastBytes("MessageName", out var messageNameBytes))
            {
                return Encoding.UTF8.GetString(messageNameBytes);
            }
            return null;
        }

        private string? ExtractMessageNameFromBody(string messageBody)
        {
            try
            {
                using var document = JsonDocument.Parse(messageBody);
                if (document.RootElement.TryGetProperty("MessageName", out var messageNameElement))
                {
                    return messageNameElement.GetString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract message name from body");
            }
            return null;
        }

        public void Dispose()
        {
            try
            {
                if (_consumer != null)
                {
                    _consumer.Close();
                    _consumer.Dispose();
                }
                _logger.LogInformation("KafkaConsumerService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing KafkaConsumerService");
            }
        }
    }
}
