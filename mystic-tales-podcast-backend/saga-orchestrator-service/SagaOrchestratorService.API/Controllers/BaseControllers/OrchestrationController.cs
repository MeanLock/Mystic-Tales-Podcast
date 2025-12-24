using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using SagaOrchestratorService.BusinessLogic.DTOs.Saga;
using SagaOrchestratorService.BusinessLogic.Services.DbServices.SagaServices;
using SagaOrchestratorService.BusinessLogic.Services.MessagingServices.interfaces;
using SagaOrchestratorService.Infrastructure.Services.Kafka;

namespace SagaOrchestratorService.API.Controllers.BaseControllers
{
    [ApiController]
    [Route("api/orchestration")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ILogger<OrchestrationController> _logger;
        private readonly SagaInstanceService _sagaInstanceService; // Assume this service is defined elsewhere
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        public OrchestrationController(ILogger<OrchestrationController> logger, SagaInstanceService sagaInstanceService, KafkaProducerService kafkaProducerService, IMessagingService messagingService)
        {
            _logger = logger;
            _sagaInstanceService = sagaInstanceService;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }
        [HttpPost("test")]
        public async Task<IActionResult> TestStartSaga(StartSagaRequest request)
        {
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage(request.Topic, request.RequestData, null, request.MessageName);
            var result = await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            if (!result)
            {
                return StatusCode(500, "Failed to initiate saga flow process.");
            }
            return Ok(new
            {
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId,
            });
        }
        [HttpGet("flows/{sagaId}")]
        public async Task<IActionResult> GetFlowDetail(Guid sagaId)
        {
            // Implement your logic here
            return Ok();
        }
        [HttpGet("result-data/{sagaId}")]
        public async Task<IActionResult> GetResponseData([FromRoute] Guid sagaId)
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
        public class StartSagaRequest
        {
            public string MessageName { get; set; } = string.Empty;
            public JObject RequestData { get; set; }
            public string Topic { get; set; } = string.Empty; // Optional, for event-driven sagas
        }
    }
}