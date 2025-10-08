using Newtonsoft.Json.Linq;
using SagaOrchestratorService.DataAccess.Enums.Saga;

namespace SagaOrchestratorService.API.DTOs.Responses
{
    /// <summary>
    /// Response DTO for saga flow details
    /// </summary>
    public class SagaDetailResponseDTO
    {
        public Guid SagaId { get; set; }

        public string FlowName { get; set; } = string.Empty;

        public string? CurrentStepName { get; set; }

        public JObject? InitialData { get; set; }

        public JObject? ResultData { get; set; }

        public SagaFlowStatusEnum FlowStatus { get; set; }

        public string? ErrorStepName { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }



    }
}