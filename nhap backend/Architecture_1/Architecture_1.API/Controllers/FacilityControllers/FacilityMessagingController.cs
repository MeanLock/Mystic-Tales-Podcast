using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Controllers.UserControllers;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.BusinessLogic.Services.DbServices.UserServices;
using Architecture_1.BusinessLogic.Services.MessagingServices.interfaces;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Architecture_1.BusinessLogic.MessageHandlers;

namespace Architecture_1.API.Controllers.FacilityControllers
{
    [Route("api/FacilityMessaging/facilities")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class FacilityMessagingController : ControllerBase
    {
        private ILogger<FacilityMessagingController> _logger;
        private readonly FacilityMessagingService _facilityMessagingService;
        private readonly IMessagingService _messagingService;
        private readonly IAppConfig _appConfig;

        public FacilityMessagingController(
            ILogger<FacilityMessagingController> logger, 
            FacilityMessagingService facilityMessagingService, 
            IMessagingService messagingService,
            IAppConfig appConfig)
        {
            _logger = logger;
            _facilityMessagingService = facilityMessagingService;
            _messagingService = messagingService;
            _appConfig = appConfig;
        }

        // [GET] /Facility/facilities
        [HttpGet]
        public async Task<IActionResult> GetFacilities()
        {
            var facilities = await _facilityMessagingService.GetFacilities();
            Console.WriteLine("\n\n\nAPP_BASE_URL: " + _appConfig.APP_BASE_URL + "\n\n\n");
            return Ok(new
            {
                Facilities = facilities
            });
        }

        // [POST] /FacilityMessaging/facilities - 🔧 WITH MESSAGING
        [HttpPost]
        public async Task<IActionResult> CreateFacility([FromBody] JToken jsonData)
        {
            dynamic facility = jsonData["Facility"].ToObject<dynamic>();

           
            // 2. Send message after successful DB operation 🔧 NEW
            var facilityCreatedEvent = new FacilityCreatedEvent
            {
                Name = facility.Name,
                Description = facility.Description,
                Image = facility.Image,
                Email_Forgot = facility.Email_Forgot,
                Email_Noti = facility.Email_Noti,
                CorrelationId = Guid.NewGuid().ToString()
            };

            // Send to Kafka via pure messaging service
            var messageSent = await _messagingService.SendMessageAsync(facilityCreatedEvent, null, "facility-events");
            
            

            return Ok(new
            {
                message = "Create facility successfully",
                // facilityId = createdFacility.Id,
                messagingSent = messageSent // 🔧 Show messaging status
            });
        }

        // [GET] /Facility/facilities/{facilityId}
        [HttpGet("{facilityId:int}")]
        public async Task<IActionResult> GetFacility(int facilityId)
        {
            var facility = await _facilityMessagingService.GetFacilityDetail(facilityId);

            return Ok(facility);
        }  

        // [PUT] /FacilityMessaging/facilities/{facilityId} - 🔧 WITH MESSAGING
        [HttpPut("{facilityId:int}")]
        public async Task<IActionResult> UpdateFacility(int facilityId, [FromBody] JToken jsonData)
        {
            dynamic facility = jsonData["Facility"].ToObject<dynamic>();

            // 1. Database operation first
            var updatedFacility = await _facilityMessagingService.UpdateFacility(facilityId, facility);

            // 2. Send message after successful DB operation 🔧 NEW
            var facilityUpdatedEvent = new FacilityUpdatedEvent
            {
                FacilityId = facilityId,
                Name = updatedFacility.Name,
                Description = updatedFacility.Description,
                CorrelationId = Guid.NewGuid().ToString()
            };

            // Send to Kafka via pure messaging service
            var messageSent = await _messagingService.SendMessageAsync(facilityUpdatedEvent, facilityId.ToString());
            
            _logger.LogInformation("Facility updated with ID: {FacilityId}, Message sent: {MessageSent}", 
                facilityId, messageSent);

            return Ok(new
            {
                message = "Update facility successfully",
                facilityId = facilityId,
                messagingSent = messageSent // 🔧 Show messaging status
            });
        }     

        // [DELETE] /FacilityMessaging/facilities/{facilityId} - 🔧 WITH MESSAGING
        [HttpDelete("{facilityId:int}")]
        public async Task<IActionResult> DeleteFacility(int facilityId)
        {
            // 1. Database operation first
            await _facilityMessagingService.DeactivateFacility(facilityId);

            // 2. Send message after successful DB operation 🔧 NEW
            var facilityDeletedEvent = new FacilityDeletedEvent
            {
                FacilityId = facilityId,
                CorrelationId = Guid.NewGuid().ToString()
            };

            // Send to Kafka via pure messaging service
            var messageSent = await _messagingService.SendMessageAsync(facilityDeletedEvent, facilityId.ToString());
            
            _logger.LogInformation("Facility deleted with ID: {FacilityId}, Message sent: {MessageSent}", 
                facilityId, messageSent);

            return Ok(new
            {
                message = "Delete facility successfully",
                facilityId = facilityId,
                messagingSent = messageSent // 🔧 Show messaging status
            });
        } 
    }
}
