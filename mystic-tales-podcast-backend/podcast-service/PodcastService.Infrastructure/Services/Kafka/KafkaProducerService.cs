using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using PodcastService.Infrastructure.Configurations.Kafka.interfaces;
using PodcastService.Infrastructure.Models.Kafka;

namespace PodcastService.Infrastructure.Services.Kafka
{
    public class KafkaProducerService : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly IKafkaClusterConfig _kafkaClusterConfig;
        private readonly IKafkaProducerConfig _kafkaProducerConfig;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(
            IKafkaClusterConfig kafkaClusterConfig,
            IKafkaProducerConfig kafkaProducerConfig,
            ILogger<KafkaProducerService> logger)
        {
            _kafkaClusterConfig = kafkaClusterConfig;
            _kafkaProducerConfig = kafkaProducerConfig;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = string.Join(",", _kafkaClusterConfig.BootstrapServers),
                ClientId = _kafkaClusterConfig.ClientId,
                SecurityProtocol = Enum.Parse<SecurityProtocol>(_kafkaClusterConfig.SecurityProtocol),
                SaslMechanism = Enum.Parse<SaslMechanism>(_kafkaClusterConfig.SaslMechanism),
                SaslUsername = _kafkaClusterConfig.SaslUsername,
                SaslPassword = _kafkaClusterConfig.SaslPassword,
                Acks = _kafkaProducerConfig.Acks == "All" ? Acks.All : Acks.Leader,
                EnableIdempotence = _kafkaProducerConfig.EnableIdempotence
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError("Producer error: {Error}", e.Reason))
                .SetLogHandler((_, log) => _logger.LogInformation("Producer log: {Message}", log.Message))
                .Build();

            _logger.LogInformation("KafkaProducerService initialized with bootstrap servers: {Servers}",
                string.Join(",", _kafkaClusterConfig.BootstrapServers));
        }

        // Fix for CS0029: Change return type from SagaCommandMessage to Task<SagaCommandMessage>
        private Task<SagaCommandMessage> PrepareSagaCommandMessage(string topic, string? key, JObject requestData, JObject? responseData, Guid? sagaId, string flowName, string messageName)
        {
            var message = new SagaCommandMessage
            {
                SagaId = sagaId ?? Guid.Empty,
                FlowName = flowName,
                MessageName = messageName,
                RequestData = requestData,
                LastStepResponseData = responseData ?? new JObject(),
                CorrelationId = Guid.NewGuid().ToString(),
                Metadata = new Dictionary<string, string>()
            };
            message.MessageId ??= Guid.NewGuid().ToString();
            message.Timestamp = message.Timestamp == default ? DateTime.UtcNow : message.Timestamp;
            message.MessageType = messageName;
            return Task.FromResult(message);
        }

        private Task<SagaEventMessage> PrepareSagaEventMessage(string topic, string? key, JObject requestData, JObject responseData, Guid? sagaId, string flowName, string messageName)
        {
            var message = new SagaEventMessage
            {
                SagaId = sagaId ?? Guid.Empty,
                FlowName = flowName,
                MessageName = messageName,
                RequestData = requestData,
                LastStepResponseData = responseData,
                CorrelationId = Guid.NewGuid().ToString(),
                Metadata = new Dictionary<string, string>()
            };
            message.MessageId ??= Guid.NewGuid().ToString();
            message.Timestamp = message.Timestamp == default ? DateTime.UtcNow : message.Timestamp;
            message.MessageType = messageName;
            return Task.FromResult(message);
        }

        private Task<StartSagaTriggerMessage> PrepareStartSagaTriggerMessage(string topic, string? key, JObject requestData, Guid? sagaId, string messageName)
        {
            var newSagaId = sagaId == null ? Guid.NewGuid() : sagaId;
            var message = new StartSagaTriggerMessage
            {
                SagaId = newSagaId ?? Guid.Empty,
                MessageName = messageName,
                RequestData = requestData,
                CorrelationId = Guid.NewGuid().ToString(),
                Metadata = new Dictionary<string, string>()
            };
            message.MessageId ??= Guid.NewGuid().ToString();
            message.Timestamp = message.Timestamp == default ? DateTime.UtcNow : message.Timestamp;
            message.MessageType = messageName;
            return Task.FromResult(message);
        }
        public async Task<KafkaMessageResult> SendMessageAsync<T>(string topic, string? key, T message)
            where T : BaseMessage
        {
            try
            {
                var envelope = new MessageEnvelope<T>
                {
                    MessageType = message.MessageType,
                    MessageId = message.MessageId,
                    Timestamp = message.Timestamp,
                    CorrelationId = message.CorrelationId,
                    Data = message,
                    Metadata = message.Metadata
                };

                var jsonMessage = System.Text.Json.JsonSerializer.Serialize(envelope);
                var kafkaMessage = new Message<string, string>
                {
                    Key = key!, // Can be null for random partitioning
                    Value = jsonMessage,
                    Timestamp = new Timestamp(DateTime.UtcNow),
                    Headers = new Headers
                    {
                        { "MessageType", Encoding.UTF8.GetBytes(message.MessageType) },
                        { "MessageId", Encoding.UTF8.GetBytes(message.MessageId) },
                        { "CorrelationId", Encoding.UTF8.GetBytes(message.CorrelationId) }
                    }
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage);

                var partitionStrategy = key != null ? "keyed partitioning" : "random partitioning";
                _logger.LogInformation("Message sent successfully - Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, MessageType: {MessageType}, Strategy: {Strategy}",
                    result.Topic, result.Partition.Value, result.Offset.Value, message.MessageType, partitionStrategy);

                return new KafkaMessageResult
                {
                    Success = true,
                    MessageId = message.MessageId,
                    Topic = result.Topic,
                    Partition = result.Partition.Value,
                    Offset = result.Offset.Value,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to topic {Topic}, MessageType: {MessageType}",
                    topic, typeof(T).Name);

                return new KafkaMessageResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Topic = topic,
                    MessageId = message.MessageId,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<KafkaMessageResult> SendSagaMessageAsync<T>(string topic, string? key, JObject requestData, JObject? responseData, Guid? sagaId, string? flowName, string messageName)
            where T : BaseMessage
        {
            try
            {
                T message;
                var newKey = key;
                if (typeof(T).Name.Equals("SagaCommandMessage"))
                {
                    // Fix CS8604: flowName is nullable, but PrepareSagaCommandMessage expects non-null
                    if (flowName == null)
                        throw new ArgumentNullException(nameof(flowName), "flowName cannot be null for SagaCommandMessage.");
                    message = (T)(object)await PrepareSagaCommandMessage(topic, key, requestData, responseData, sagaId, flowName, messageName);
                }
                else if (typeof(T).Name.Equals("SagaEventMessage"))
                {
                    if (flowName == null)
                        throw new ArgumentNullException(nameof(flowName), "flowName cannot be null for SagaEventMessage.");
                    // responseData is not nullable for SagaEventMessage
                    if (responseData == null)
                        throw new ArgumentNullException(nameof(responseData), "responseData cannot be null for SagaEventMessage.");
                    message = (T)(object)await PrepareSagaEventMessage(topic, key, requestData, responseData, sagaId, flowName, messageName);
                }
                else if (typeof(T).Name.Equals("StartSagaTriggerMessage"))
                {
                    message = (T)(object)await PrepareStartSagaTriggerMessage(topic, key, requestData, sagaId, messageName);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported message type: {typeof(T).Name}");
                }

                var jsonMessage = SerializeToJson(message);
                var kafkaMessage = new Message<string, string>
                {
                    Key = key ?? newKey, // Can be null for random partitioning
                    Value = jsonMessage,
                    Timestamp = new Timestamp(DateTime.UtcNow),
                    Headers = new Headers
                    {
                        { "MessageType", Encoding.UTF8.GetBytes(message.MessageType) },
                        { "MessageId", Encoding.UTF8.GetBytes(message.MessageId) },
                        { "CorrelationId", Encoding.UTF8.GetBytes(message.CorrelationId) }
                    }
                };

                var result = await _producer.ProduceAsync(topic, kafkaMessage);

                var partitionStrategy = key != null ? "keyed partitioning" : "random partitioning";
                _logger.LogInformation("Message sent successfully - Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, MessageType: {MessageType}, Strategy: {Strategy}",
                    result.Topic, result.Partition.Value, result.Offset.Value, message.MessageType, partitionStrategy);

                return new KafkaMessageResult
                {
                    Success = true,
                    MessageId = message.MessageId,
                    Topic = result.Topic,
                    Partition = result.Partition.Value,
                    Offset = result.Offset.Value,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to topic {Topic}, MessageType: {MessageType}",
                    topic, typeof(T).Name);

                return new KafkaMessageResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Topic = topic,
                    MessageId = sagaId.ToString(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public void Dispose()
        {
            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _logger.LogInformation("KafkaProducerService disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing KafkaProducerService");
            }
        }
        protected string SerializeToJson<T>(T data) where T : class
        {
            try
            {
                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serialize data: {Data}", data);
                return "{}";
            }
        }
    }
}
