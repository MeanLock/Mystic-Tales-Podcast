using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.Infrastructure.Services.Audio.AcoustID;
using UserService.Infrastructure.Services.Kafka;

namespace UserService.API.Controllers.BaseControllers
{
    [Route("api/auth")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AuthController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;


        public AuthController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient, KafkaProducerService kafkaProducerService, IMessagingService messagingService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
        }

        ///api/user-service/api/auth/register/customer
        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromForm] CustomerRegisterRequestDTO customerRegisterRequestDTO)
        {
            var RegisterInfo = JsonConvert.DeserializeObject<CustomerRegisterInfoDTO>(customerRegisterRequestDTO.RegisterInfo);

            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", JObject.FromObject(RegisterInfo), null, "user-registration-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return Ok(new
            {
                // customerRegisterRequestDTO = customerRegisterRequestDTO.RegisterInfo,
                // RegisterInfo = RegisterInfo,
                startSagaTriggerMessage,
                SagaInstanceId = startSagaTriggerMessage.SagaInstanceId
            }
            );
        }



    }
}
