using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestratorService.Infrastructure.Models.Kafka
{
    // For step outcomes (emits). messageType should be like "create-order.success"
    public class SagaEventMessage : SagaBaseMessage
    {
        public string FlowName { get; set; } = string.Empty;
        public JObject RequestData { get; set; } = new();
        public JObject LastStepResponseData { get; set; } = new();
    }
}
