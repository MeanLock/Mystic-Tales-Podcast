using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Controllers.UserControllers;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.BusinessLogic.Services.DbServices.UserServices;
using Architecture_1.Common.AppConfigurations.App.interfaces;

namespace Architecture_1.API.Controllers.FacilityControllers
{
    [Route("api/Facility/facilities")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class FacilityController : ControllerBase
    {
        private ILogger<FacilityController> _logger;
        private readonly FacilityService _facilityService;
        private readonly IAppConfig _appConfig;

        public FacilityController(ILogger<FacilityController> logger, FacilityService facilityService, IAppConfig appConfig)
        {
            _logger = logger;
            _facilityService = facilityService;
            _appConfig = appConfig;
        }

        // [GET] /Facility/facilities
        [HttpGet]
        public async Task<IActionResult> GetFacilities()
        {
            var facilities = await _facilityService.GetFacilities();
            Console.WriteLine("\n\n\nAPP_BASE_URL: " + _appConfig.APP_BASE_URL + "\n\n\n");
            return Ok(new
            {
                Facilities = facilities
            });
        }

        // [POST] /Facility/facilities
        [HttpPost]
        public async Task<IActionResult> CreateFacility([FromBody] JToken jsonData)
        {
            dynamic facility = jsonData["Facility"].ToObject<dynamic>();

            await _facilityService.CreateFacility(facility);

            return Ok(new
            {
                // message = "Tạo cơ sở vật chất thành công",
                message = "Create facility successfully",
            });
        }

        // [GET] /Facility/facilities/{facilityId}
        [HttpGet("{facilityId:int}")]
        public async Task<IActionResult> GetFacility(int facilityId)
        {
            var facility = await _facilityService.GetFacilityDetail(facilityId);

            return Ok(facility);
        }  

        // [PUT] /Facility/facilities/{facilityId}
        [HttpPut("{facilityId:int}")]
        public async Task<IActionResult> UpdateFacility(int facilityId, [FromBody] JToken jsonData)
        {
            dynamic facility = jsonData["Facility"].ToObject<dynamic>();

            await _facilityService.UpdateFacility(facilityId, facility);

            return Ok(new
            {
                // message = "Cập nhật cơ sở vật chất thành công",
                message = "Update facility successfully",
            });
        }     

        // [DELETE] /Facility/facilities/{facilityId}
        [HttpDelete("{facilityId:int}")]
        public async Task<IActionResult> DeleteFacility(int facilityId)
        {
            await _facilityService.DeactivateFacility(facilityId);

            return Ok(new
            {
                // message = "Xóa cơ sở vật chất thành công",
                message = "Delete facility successfully",
            });
        } 
    }
}
