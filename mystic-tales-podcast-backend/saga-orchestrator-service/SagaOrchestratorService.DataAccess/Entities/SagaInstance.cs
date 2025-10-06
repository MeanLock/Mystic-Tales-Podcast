using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SagaOrchestratorService.DataAccess.Enums.Saga;

namespace SagaOrchestratorService.DataAccess.Entities
{
    public class SagaInstance
    {
        public Guid Id { get; set; }
        public string FlowName { get; set; }
        public string? CurrentStepName { get; set; }
        public string? InitialData { get; set; }
        public string? ResultData { get; set; }
        public SagaFlowStatusEnum FlowStatus { get; set; }
        public string? ErrorStepName { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

}
