using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.Infrastructure.Models.Kafka
{
    // For step commands (messageType == MessageName)
    public class SagaCommandMessage : SagaBaseMessage
    {
        public string FlowName { get; set; } = string.Empty;
        public JObject RequestData { get; set; } = new();
        public JObject LastStepResponseData { get; set; } = new();
    }
}
