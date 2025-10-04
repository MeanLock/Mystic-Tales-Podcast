using Microsoft.AspNetCore.Mvc;

namespace SagaOrchestratorService.API.Controllers.BaseControllers
{
    [ApiController]
    [Route("api/orchestration")]
    public class OrchestrationController : ControllerBase
    {
        [HttpGet("flows/{sagaId}")]
        public async Task<IActionResult> GetFlowDetail(Guid sagaId)
        {
            // Implement your logic here
            return Ok();
        }
    }
}