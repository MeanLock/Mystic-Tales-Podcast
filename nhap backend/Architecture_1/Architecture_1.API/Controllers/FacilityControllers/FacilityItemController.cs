using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.API.Controllers.FacilityControllers
{
    [Route("api/Facility/items")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class FacilityItemController : ControllerBase
    {
        private ILogger<FacilityItemController> _logger;
        private readonly FacilityService _facilityService;
        private readonly FacilityItemService _facilityItemService;

        public FacilityItemController(
            ILogger<FacilityItemController> logger,
            FacilityService facilityService,
            FacilityItemService facilityItemService
            )
        {
            _logger = logger;
            _facilityService = facilityService;
            _facilityItemService = facilityItemService;
        }

        // [GET] /Facility/items
        [HttpGet]
        public async Task<IActionResult> GetFacilityItems()
        {
            var facilityItems = await _facilityItemService.GetFacilityItems();

            return Ok(new
            {
                Items = facilityItems
            });
        }

        // [POST] /Facility/items
        [HttpPost]
        public async Task<IActionResult> CreateFacilityItem([FromBody] JToken jsonData)
        {
            dynamic facilityItem = jsonData["Item"].ToObject<dynamic>();


            await _facilityItemService.CreateFacilityItem(facilityItem);

            return Ok(new
            {
                // message = "Tạo vật tư thành công",
                message = "Create facility item successfully"
            });
        }

        // [GET] /Facility/items/{itemId}
        [HttpGet("{itemId:int}")]
        public async Task<IActionResult> GetFacilityItem(int itemId)
        {
            var facilityItem = await _facilityItemService.GetFacilityItemDetail(itemId);

            return Ok(facilityItem);
        }

        // [PUT] /Facility/items/{itemId}
        [HttpPut("{itemId:int}")]
        public async Task<IActionResult> UpdateFacilityItem(int itemId, [FromBody] JToken jsonData)
        {
            dynamic facilityItem = jsonData["Item"].ToObject<dynamic>();

            await _facilityItemService.UpdateFacilityItem(itemId, facilityItem);

            return Ok(new
            {
                // message = "Cập nhật vật tư thành công",
                message = "Update facility item successfully"
            });
        }

        // [DELETE] /Facility/items/{itemId}
        [HttpDelete("{itemId:int}")]
        public async Task<IActionResult> DeleteFacilityItem(int itemId)
        {

            await _facilityItemService.DeleteFacilityItem(itemId);

            return Ok(new
            {
                // message = "Xóa vật tư thành công",
                message = "Delete facility item successfully",
            });
        }

        // [PUT] /Facility/items/{itemId}/add
        [HttpPut("{itemId:int}/add")]
        public async Task<IActionResult> AddFacilityItem(int itemId, [FromBody] JToken jsonData)
        {
            int facilityItem = jsonData["Count"].ToObject<int>();

            await _facilityItemService.AddFacilityItem(itemId, facilityItem);

            return Ok(new
            {
                // message = "Thêm vật tư vào kho thành công",
                message = "Add facility item successfully"
            });
        }

        // [PUT] /Facility/items/{itemId}/subtract
        [HttpPut("{itemId:int}/subtract")]
        public async Task<IActionResult> SubtractFacilityItem(int itemId, [FromBody] JToken jsonData)
        {
            int facilityItem = jsonData["Count"].ToObject<int>();

            await _facilityItemService.SubtractFacilityItem(itemId, facilityItem);

            return Ok(new
            {
                // message = "Xóa vật tư khỏi kho thành công",
                message = "Subtract facility item successfully"
            });
        }

        // [GET] /Facility/items/{itemId}/majors
        [HttpGet("{itemId:int}/majors")]
        public async Task<IActionResult> GetFacilityItemMajors(int itemId)
        {
            var majors = await _facilityItemService.GetFacilityItemAssignment(itemId);

            return Ok(new
            {
                FacilityItemAssignments = majors
            });
        }

        // [POST] /Facility/items/{itemId}/majors
        [HttpPost("{itemId:int}/majors")]
        public async Task<IActionResult> CreateFacilityItemMajor(int itemId, [FromBody] JToken jsonData)
        {
            int Count = jsonData["Count"].ToObject<int>();
            List<int> MajorIds = jsonData["MajorIds"].ToObject<List<int>>();
        

            await _facilityItemService.CreateFacilityItemAssignment(itemId, Count, MajorIds);

            return Ok(new
            {
                // message = "Thêm ngành học vào vật tư thành công",
                message = "Create facility item assignment successfully"
            });
        }
    }
}
