using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Architecture_1.API.Controllers.FacilityControllers;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices;
using Architecture_1.API.Filters.ExceptionFilters;
using Newtonsoft.Json.Linq;

namespace Architecture_1.API.Controllers.FacilityMajorControllers
{
    [Route("api/Major/assignments")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class FacilityMajorAssignmentController : ControllerBase
    {
        private ILogger<FacilityMajorAssignmentController> _logger;
        private readonly MajorService _majorService;
        private readonly MajorAssignmentService _majorAssignmentService;
        private readonly MajorServicesService _majorServicesService;

        public FacilityMajorAssignmentController(ILogger<FacilityMajorAssignmentController> logger, MajorService majorService, MajorAssignmentService majorAssignmentService, MajorServicesService majorServicesService)
        {
            _logger = logger;
            _majorService = majorService;
            _majorAssignmentService = majorAssignmentService;
            _majorServicesService = majorServicesService;
        }

        // [GET] /Major/assignments/major-head/{accountId}/staffs
        [HttpGet("major-head/{accountId:int}/staffs")]
        public async Task<IActionResult> GetMajorHeadStaffs(int accountId)
        {
            var majors = await _majorAssignmentService.GetMajorHeadStaffs(accountId);

            return Ok(new
            {
                Majors = majors
            });
        }

        // [GET] /Major/assignments/major/{majorId}/staffs
        [HttpGet("major/{majorId:int}/staffs")]
        public async Task<IActionResult> GetMajorStaffs(int majorId)
        {
            var accounts = await _majorAssignmentService.GetMajorStaffs(majorId, false);

            return Ok(new
            {
                Accounts = accounts
            });
        }

        // [GET] /Major/assignments/staff/{accountId}
        [HttpGet("staff/{accountId:int}")]
        public async Task<IActionResult> GetStaffAssignments(int accountId)
        {
            var assignments = await _majorAssignmentService.GetStaffAssignments(accountId);

            return Ok(new
            {
                MajorAssignments = assignments
            });
        }

        // [POST] /Major/assignments/staff/{accountId}
        [HttpPost("staff/{accountId:int}")]
        public async Task<IActionResult> AssignStaff(int accountId, [FromBody] JToken jsonData)
        {
            List<int> MajorIds = jsonData["MajorIds"].ToObject<List<int>>();

            await _majorAssignmentService.AssignStaff(accountId, MajorIds);

            return Ok(new
            {
                // message = "Phân công nhân viên thành công",
                message = "Assign staff successfully"
            });
        }

        // [PUT] /Major/assignments/staff/{accountId}/majors/{majorId}
        [HttpPut("staff/{accountId:int}/majors/{majorId:int}")]
        public async Task<IActionResult> UpdateStaffAssignment(int accountId, int majorId, [FromBody] JToken jsonData)
        {
            string workDescription = jsonData["WorkDescription"].ToString();

            await _majorAssignmentService.UpdateStaffAssignment(accountId, majorId, workDescription);

            return Ok(new
            {
                // message = "Cập nhật phân công nhân viên thành công",
                message = "Update staff assignment successfully"
            });
        }

        // [DELETE] /Major/assignments/staff/{accountId}/majors/{majorId}
        [HttpDelete("staff/{accountId:int}/majors/{majorId:int}")]
        public async Task<IActionResult> DeleteStaffAssignment(int accountId, int majorId)
        {
            await _majorAssignmentService.DeleteStaffAssignment(accountId, majorId);

            return Ok(new
            {
                // message = "Xóa phân công nhân viên thành công",
                message = "Delete staff assignment successfully"
            });
        }
    }
}
