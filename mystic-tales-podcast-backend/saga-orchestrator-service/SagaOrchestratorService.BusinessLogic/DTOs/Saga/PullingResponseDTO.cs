using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestratorService.BusinessLogic.DTOs.Saga
{
    public class PullingResponseDTO
    {
        public Guid SagaId { get; set; }
        public string FlowStatus { get; set; } = string.Empty;
        public string ResultData { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
