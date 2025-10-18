using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SagaOrchestratorService.DataAccess.Entities.SqlServer;
using SagaOrchestratorService.DataAccess.Enums.Saga;
using SagaOrchestratorService.DataAccess.Repositories.interfaces;

namespace SagaOrchestratorService.BusinessLogic.Services.DbServices.SagaServices
{
    public class SagaInstanceService
    {
        private readonly IGenericRepository<SagaInstance> _sagaInstanceGenericRepository;
        private readonly IGenericRepository<SagaStepExecution> _stepExecutionGenericRepository;
        private readonly ILogger<SagaInstanceService> _logger;

        public SagaInstanceService(
            IGenericRepository<SagaInstance> sagaInstanceGenericRepository,
            IGenericRepository<SagaStepExecution> stepExecutionGenericRepository,
            ILogger<SagaInstanceService> logger)
        {
            _sagaInstanceGenericRepository = sagaInstanceGenericRepository;
            _stepExecutionGenericRepository = stepExecutionGenericRepository;
            _logger = logger;
        }

        public async Task<SagaInstance> CreateSagaInstanceAsync(Guid sagaId, string flowName, string initialDataJson, string firstStepName)
        {
            var sagaInstance = new SagaInstance
            {
                Id = sagaId,
                FlowName = flowName,
                CurrentStepName = firstStepName,
                InitialData = initialDataJson,
                ResultData = null,
                FlowStatus = Enum.GetName(typeof(SagaFlowStatusEnum), SagaFlowStatusEnum.RUNNING),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _sagaInstanceGenericRepository.CreateAsync(sagaInstance);
            _logger.LogInformation("Created saga instance: {SagaId}, Flow: {FlowName}", sagaId, flowName);
            
            return created ?? sagaInstance;
        }

        public async Task<SagaStepExecution> CreateStepExecutionAsync(Guid sagaId, string stepName, string? topicName, string? requestData)
        {
            var stepExecution = new SagaStepExecution
            {
                Id = Guid.NewGuid(),
                SagaInstanceId = sagaId,
                StepName = stepName,
                TopicName = topicName,
                StepStatus = Enum.GetName(typeof(SagaFlowStatusEnum), SagaFlowStatusEnum.RUNNING),
                RequestData = requestData,
                ResponseData = null,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _stepExecutionGenericRepository.CreateAsync(stepExecution);
            _logger.LogInformation("Created step execution: {SagaId}, Step: {StepName}", sagaId, stepName);
            
            return created ?? stepExecution;
        }

        public async Task UpdateStepExecutionStatusAsync(Guid sagaId, string stepName, SagaStepStatusEnum status, string? requestData, string? responseData, string? errorMessage)
        {
            var stepExecutions = await GetStepExecutionsAsync(sagaId);
            var stepExecution = stepExecutions.FirstOrDefault(s => s.StepName == stepName && s.StepStatus.Equals(nameof(SagaStepStatusEnum.RUNNING)));

            if (stepExecution != null)
            {
                stepExecution.StepStatus = Enum.GetName(typeof(SagaFlowStatusEnum), status);
                
                // Update request data if provided
                if (requestData != null)
                    stepExecution.RequestData = requestData;
                
                // Update response data if provided
                if (responseData != null)
                    stepExecution.ResponseData = responseData;
                
                // Update error message if provided
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    stepExecution.ErrorMessage = errorMessage;

                await _stepExecutionGenericRepository.UpdateAsync(stepExecution.Id, stepExecution);
                _logger.LogInformation("Updated step execution: {SagaId}, Step: {StepName}, Status: {Status}", sagaId, stepName, status);
            }
            else
            {
                _logger.LogWarning("Step execution not found for update: {SagaId}, Step: {StepName}", sagaId, stepName);
            }
        }

        public async Task UpdateSagaCurrentStepAsync(Guid sagaId, string? currentStepName)
        {
            var sagaInstance = await GetSagaInstanceAsync(sagaId);
            if (sagaInstance != null)
            {
                sagaInstance.CurrentStepName = currentStepName;
                sagaInstance.UpdatedAt = DateTime.UtcNow;
                await _sagaInstanceGenericRepository.UpdateAsync(sagaInstance.Id, sagaInstance);
                _logger.LogInformation("Updated saga current step: {SagaId}, Step: {StepName}", sagaId, currentStepName);
            }
        }

        public async Task<bool> CheckAndUpdateSagaCompletionAsync(Guid sagaId)
        {
            var stepExecutions = await GetStepExecutionsAsync(sagaId);

            // Check if all steps are completed successfully
            //var allSuccess = stepExecutions.All(s => s.StepStatus == SagaStepStatusEnum.SUCCESS);
            var anyFailed = stepExecutions.Any(s => s.StepStatus.Equals(Enum.GetName(typeof(SagaFlowStatusEnum), SagaFlowStatusEnum.FAILED)));


            if (!anyFailed)
            {
                // Only update status to SUCCESS, don't update ResultData here
                // ResultData will be updated by the handler
                var sagaInstance = await GetSagaInstanceAsync(sagaId);
                if (sagaInstance != null)
                {
                    sagaInstance.FlowStatus = Enum.GetName(typeof(SagaFlowStatusEnum), SagaFlowStatusEnum.SUCCESS);
                    sagaInstance.UpdatedAt = DateTime.UtcNow;
                    sagaInstance.CompletedAt = DateTime.UtcNow;
                    await _sagaInstanceGenericRepository.UpdateAsync(sagaInstance.Id, sagaInstance);
                    
                    _logger.LogInformation("Saga completed successfully: {SagaId}", sagaId);
                    return true;
                }
            }

            return false;
        }

        public async Task UpdateSagaResultData(Guid sagaId, string resultDataJson)
        {
            var sagaInstance = await GetSagaInstanceAsync(sagaId);
            if (sagaInstance != null)
            {
                sagaInstance.ResultData = resultDataJson;
                sagaInstance.UpdatedAt = DateTime.UtcNow;
                await _sagaInstanceGenericRepository.UpdateAsync(sagaInstance.Id, sagaInstance);
                _logger.LogInformation("Updated saga result data: {SagaId}", sagaId);
            }
        }

        public async Task UpdateSagaStatusAsync(Guid sagaId, SagaFlowStatusEnum status, string? resultDataJson, string? errorStepName, string? errorMessage)
        {
            var sagaInstance = await GetSagaInstanceAsync(sagaId);
            if (sagaInstance != null)
            {
                sagaInstance.FlowStatus = Enum.GetName(typeof(SagaFlowStatusEnum), status);
                sagaInstance.UpdatedAt = DateTime.UtcNow;

                // Update result data if provided
                if (resultDataJson != null)
                    sagaInstance.ResultData = resultDataJson;
                
                // Update error step name if provided
                if (!string.IsNullOrWhiteSpace(errorStepName))
                    sagaInstance.ErrorStepName = errorStepName;
                
                // Update error message if provided
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    sagaInstance.ErrorMessage = errorMessage;

                // Set completion time for final states
                if (status == SagaFlowStatusEnum.SUCCESS || status == SagaFlowStatusEnum.FAILED)
                    sagaInstance.CompletedAt = DateTime.UtcNow;

                await _sagaInstanceGenericRepository.UpdateAsync(sagaInstance.Id, sagaInstance);
                _logger.LogInformation("Updated saga status: {SagaId}, Status: {Status}", sagaId, status);
            }
            else
            {
                _logger.LogWarning("Saga instance not found for status update: {SagaId}", sagaId);
            }
        }

        public async Task<SagaInstance?> GetSagaInstanceAsync(Guid sagaId)
        {
            return await _sagaInstanceGenericRepository.FindByIdAsync(sagaId);
        }

        public async Task<List<SagaStepExecution>> GetStepExecutionsAsync(Guid sagaId)
        {
            var allExecutions = _stepExecutionGenericRepository.FindAll();
            return allExecutions.Where(s => s.SagaInstanceId == sagaId).ToList();
        }
    }
}