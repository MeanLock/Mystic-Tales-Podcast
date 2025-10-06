using Newtonsoft.Json.Linq;

namespace SystemConfigurationService.Infrastructure.Models.Kafka
{
    // For step outcomes (emits). messageType should be like "create-order.success"
    public class SagaEventMessage : BaseMessage
    {
        public Guid SagaId { get; set; }
        public string FlowName { get; set; } = string.Empty;
        public string MessageName { get; set; } = string.Empty; // equals MessageType (emit)
        public JObject RequestData { get; set; } = new();
        public JObject LastStepResponseData { get; set; } = new();
    }
}
