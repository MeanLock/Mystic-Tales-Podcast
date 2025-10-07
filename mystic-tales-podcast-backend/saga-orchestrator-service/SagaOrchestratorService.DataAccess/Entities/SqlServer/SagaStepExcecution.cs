using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SagaOrchestratorService.DataAccess.Enums.Saga;

namespace SagaOrchestratorService.DataAccess.Entities.SqlServer
{
    public class SagaStepExcecution
    {
        public int Id { get; set; }
        public Guid SagaInstanceId { get; set; }
        public string StepName { get; set; }
        public string? TopicName { get; set; }
        public SagaStepStatusEnum StepStatus { get; set; }
        public string? RequestData { get; set; }
        public string? ResponseData { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
}
