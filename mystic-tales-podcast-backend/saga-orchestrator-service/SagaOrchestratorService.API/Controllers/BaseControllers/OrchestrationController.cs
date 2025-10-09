using Microsoft.AspNetCore.Mvc;
using SagaOrchestratorService.BusinessLogic.DTOs.Saga;
using SagaOrchestratorService.BusinessLogic.Services.DbServices.SagaServices;

namespace SagaOrchestratorService.API.Controllers.BaseControllers
{
    [ApiController]
    [Route("api/orchestration")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ILogger<OrchestrationController> _logger;
        private readonly SagaInstanceService _sagaInstanceService; // Assume this service is defined elsewhere
        public OrchestrationController(ILogger<OrchestrationController> logger, SagaInstanceService sagaInstanceService)
        {
            _logger = logger;
            _sagaInstanceService = sagaInstanceService;
        }
        [HttpGet("flows/{sagaId}")]
        public async Task<IActionResult> GetFlowDetail(Guid sagaId)
        {
            // Implement your logic here
            return Ok();
        }
        [HttpGet("result-data/{sagaId}")]
        public async Task<IActionResult> GetResponseData(Guid sagaId)
        {
            var sagaInstance = await _sagaInstanceService.GetSagaInstanceAsync(sagaId);
            if (sagaInstance == null)
            {
                return NotFound($"Saga instance with ID {sagaId} not found.");
            }
            var response = new PullingResponseDTO
            {
                SagaId = sagaInstance.Id,
                FlowStatus = sagaInstance.FlowStatus.ToString(),
                ResultData = sagaInstance.ResultData ?? string.Empty,
                ErrorMessage = sagaInstance.ErrorMessage ?? string.Empty
            };
            return Ok(response);
        }
    }
}