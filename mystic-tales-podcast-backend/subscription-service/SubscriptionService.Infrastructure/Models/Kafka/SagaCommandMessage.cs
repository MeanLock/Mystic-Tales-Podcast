using Newtonsoft.Json.Linq;

namespace SubscriptionService.Infrastructure.Models.Kafka
{
    // For step commands (messageType == MessageName)
    public class SagaCommandMessage : BaseMessage
    {
        public Guid SagaId { get; set; }
        public string FlowName { get; set; } = string.Empty;
        public string MessageName { get; set; } = string.Empty; // equals MessageType
        public JObject RequestData { get; set; } = new();
        public JObject LastStepResponseData { get; set; } = new();
    }
}
