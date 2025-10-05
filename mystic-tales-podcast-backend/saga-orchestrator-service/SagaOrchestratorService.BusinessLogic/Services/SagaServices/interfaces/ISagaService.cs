using SagaOrchestratorService.DataAccess.Entities;
using SagaOrchestratorService.DataAccess.Enums.Saga;

namespace SagaOrchestratorService.BusinessLogic.Services.SagaServices.interfaces
{
    public interface ISagaService
    {
        Task<SagaInstance> CreateSagaInstanceAsync(Guid sagaId, string flowName, string initialDataJson, string firstStepName);
        Task<SagaStepExcecution> CreateStepExecutionAsync(Guid sagaId, string stepName, string? topicName, Dictionary<string, object>? requestData);
        Task UpdateStepExecutionStatusAsync(Guid sagaId, string stepName, StepStatus status, Dictionary<string, object>? requestData = null, Dictionary<string, object>? responseData = null, string? errorMessage = null);
        Task UpdateSagaCurrentStepAsync(Guid sagaId, string? currentStepName);
        Task<bool> CheckAndUpdateSagaCompletionAsync(Guid sagaId);
        Task UpdateSagaStatusAsync(Guid sagaId, SagaFlowStatusEnum status, string? resultDataJson = null, string? errorStepName = null, string? errorMessage = null);
        Task<SagaInstance?> GetSagaInstanceAsync(Guid sagaId);
        Task<List<SagaStepExcecution>> GetStepExecutionsAsync(Guid sagaId);
        Task UpdateSagaResultData(Guid sagaId, string resultDataJson);
    }
}