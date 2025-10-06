using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Infrastructure.Models.Kafka
{
    // For step commands (messageType == MessageName)
    public class SagaBaseMessage
    {
        public Guid SagaInstanceId { get; set; }
        public string MessageTopic { get; set; } = string.Empty;
        public string MessageName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
