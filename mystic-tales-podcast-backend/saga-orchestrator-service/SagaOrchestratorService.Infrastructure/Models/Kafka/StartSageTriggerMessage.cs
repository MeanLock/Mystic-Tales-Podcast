using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestratorService.Infrastructure.Models.Kafka
{
    // To start a flow by sending messageType == flow name (handled by FlowMessageHandler).
    // No SagaId here; the receiving handler will generate a new one.
    public class StartSagaTriggerMessage : SagaBaseMessage
    {
        public JObject RequestData { get; set; } = new();
    }
}
