using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SagaOrchestratorService.DataAccess.Enums.Saga;

namespace SagaOrchestratorService.DataAccess.Entities
{
    public class SagaStepExcecution
    {
        public int Id { get; set; }
        public Guid SagaId { get; set; }
        public string StepName { get; set; }
        public string? TopicName { get; set; }
        public SagaStepStatusEnum StepStatus { get; set; }
        public Dictionary<string, object>? RequestData { get; set; } = new();
        public Dictionary<string, object>? responseData { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
}
