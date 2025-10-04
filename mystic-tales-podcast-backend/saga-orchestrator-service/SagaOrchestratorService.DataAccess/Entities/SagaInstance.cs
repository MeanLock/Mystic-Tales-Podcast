using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestratorService.DataAccess.Entities
{
    public class SagaInstance
    {
        public Guid SagaId { get; set; }
        public string FlowName { get; set; }
        public string? CurrentStepName { get; set; }
        public string? InitialData { get; set; }
        public string? ResultData { get; set; }
        public SagaStatus FlowStatus { get; set; }
        public string? ErrorStepName { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
    public enum SagaStatus
    {
        RUNNING,
        SUCCESS,
        FAILED
    }
}
