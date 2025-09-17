using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices;

namespace Architecture_1.API.Controllers.FacilityMajorControllers
{
    [Route("api/Major/majors")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class FacilityMajorController : ControllerBase
    {
        private ILogger<FacilityMajorController> _logger;
        private readonly MajorService _majorService;
        private readonly MajorAssignmentService _majorAssignmentService;
        private readonly MajorServicesService _majorServicesService;

        public FacilityMajorController(ILogger<FacilityMajorController> logger, MajorService majorService, MajorAssignmentService majorAssignmentService, MajorServicesService majorServicesService)
        {
            _logger = logger;
            _majorService = majorService;
            _majorAssignmentService = majorAssignmentService;
            _majorServicesService = majorServicesService;
        }

        // [GET] /Major/majors
        [HttpGet]
        // [Authorize(AuthenticationSchemes = "PublicKeyAuth", Roles = "Campus Manager, Facility Major Head")]
        public async Task<IActionResult> Get()
        {
            var majors = await _majorService.GetMajors();

            return Ok(new
            {
                Majors = majors
            });
        }


        // [POST] /Major/majors
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JToken jsonData)
        {
            dynamic major = jsonData["Major"].ToObject<dynamic>();

            await _majorService.CreateMajor(major);

            return Ok(new
            {
                // message = "Tạo major thành công",
                message = "Create facility major successfully"
            });
        }

        // [GET] /Major/majors/features
        [HttpGet("features")]
        public async Task<IActionResult> GetFeatures()
        {
            var features = await _majorService.GetAllFeatureMajors();

            return Ok(new
            {
                Features = features
            });
        }

        // [GET] /Major/majors/major-head/{accountId}/majors
        [HttpGet("major-head/{accountId:int}/majors")]
        public async Task<IActionResult> GetMajorHeadMajors(int accountId)
        {
            var majors = await _majorService.GetMajorHeadMajors(accountId);

            return Ok(new
            {
                Majors = majors
            });
        }

        // [GET] /Major/majors/assignee/{accountId}/majors
        [HttpGet("assignee/{accountId:int}/majors")]
        public async Task<IActionResult> GetAssigneeMajors(int accountId)
        {
            var majors = await _majorService.GetAssigneeMajors(accountId);

            return Ok(new
            {
                Majors = majors
            });
        }

        // [GET] /Major/majors/major-head/{accountId}/requesters 
        [HttpGet("major-head/{accountId:int}/requesters")]
        public async Task<IActionResult> GetMajorHeadRequesters(int accountId)
        {
            var requesters = await _majorService.GetMajorHeadRequesters(accountId);

            return Ok(new
            {
                Accounts = requesters
            });
        }

        // [GET] /Major/majors/{majorId}
        [HttpGet("{majorId:int}")]
        public async Task<IActionResult> Get(int majorId)
        {
            var major = await _majorService.GetMajorDetail(majorId);

            return Ok(major);
        }

        // [PUT] /Major/majors/{majorId}
        [HttpPut("{majorId:int}")]
        public async Task<IActionResult> Update(int majorId, [FromBody] JToken jsonData)
        {
            dynamic major = jsonData["Major"].ToObject<dynamic>();

            await _majorService.UpdateMajor(majorId, major);

            return Ok(new
            {
                // message = "Cập nhật major thành công",
                message = "Update facility major successfully"
            });
        }

        // [DELETE] /Major/majors/{majorId}
        [HttpDelete("{majorId:int}")]
        public async Task<IActionResult> Delete(int majorId)
        {
            await _majorService.DeactivateMajor(majorId, true);

            return Ok(new
            {
                // message = "Xóa major thành công",
                message = "Delete facility major successfully",
            });
        }

        // [GET] /Major/majors/feedbacks 
        [HttpGet("feedbacks")]
        public async Task<IActionResult> GetFeedbacks()
        {
            var feedbacks = await _majorService.GetAllFeedbacks();

            return Ok(new
            {
                Feedbacks = feedbacks
            });
        }

        // [GET] /Major/majors/major-head/{accountId}/feedbacks
        [HttpGet("major-head/{accountId:int}/feedbacks")]
        public async Task<IActionResult> GetMajorHeadFeedbacks(int accountId)
        {
            var feedbacks = await _majorService.GetMajorHeadFeedbacks(accountId);

            return Ok(new
            {
                Feedbacks = feedbacks
            });
        }

        // [GET] /Major/majors/{majorId}/feedbacks
        [HttpGet("{majorId:int}/feedbacks")]
        public async Task<IActionResult> GetFeedbacksByMajor(int majorId)
        {
            var feedbacks = await _majorService.GetFeedbacksByMajor(majorId);

            return Ok(new
            {
                Feedbacks = feedbacks
            });
        }

        // [POST] /Major/majors/{majorId}/account/{accountId}/feedbacks
        [HttpPost("{majorId:int}/account/{accountId:int}/feedbacks")]
        public async Task<IActionResult> CreateFeedback(int majorId, int accountId, [FromBody] JToken jsonData)
        {
            dynamic feedback = jsonData["Feedback"].ToObject<dynamic>();

            await _majorService.CreateFeedback(majorId, accountId, feedback);

            return Ok(new
            {
                // message = "Feedback thành công",
                message = "Create feedback successfully"
            });
        }

        // [DELETE] /Major/majors/feedbacks/{feedbackId}
        [HttpDelete("feedbacks/{feedbackId:int}")]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
        {
            await _majorService.DeactivateFeedback(feedbackId, true);

            return Ok(new
            {
                // message = "Xóa feedback thành công",
                message = "Delete feedback successfully",
            });
        }

        // [GET] /Major/majors/reports
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _majorService.GetAllReports();

            return Ok(new
            {
                Reports = reports
            });
        }

        // [GET] /Major/majors/major-head/{accountId}/reports
        [HttpGet("major-head/{accountId:int}/reports")]
        public async Task<IActionResult> GetMajorHeadReports(int accountId)
        {
            var reports = await _majorService.GetMajorHeadReports(accountId);

            return Ok(new
            {
                Reports = reports
            });
        }

        
        // [GET] /Major/majors/{majorId}/reports
        [HttpGet("{majorId:int}/reports")]
        public async Task<IActionResult> GetReportsByMajor(int majorId)
        {
            var reports = await _majorService.GetReportsByMajor(majorId);

            return Ok(new
            {
                Reports = reports
            });
        }

        // [POST] /Major/majors/{majorId}/account/{accountId}/reports 
        [HttpPost("{majorId:int}/account/{accountId:int}/reports")]
        public async Task<IActionResult> CreateReport(int majorId, int accountId, [FromBody] JToken jsonData)
        {
            dynamic report = jsonData["Report"].ToObject<dynamic>();

            await _majorService.CreateReport(majorId, accountId, report);

            return Ok(new
            {
                // message = "Report thành công",
                message = "Create report successfully"
            });
        }

        // [GET] /Major/majors/reports/{reportId}
        [HttpGet("reports/{reportId:int}")]
        public async Task<IActionResult> GetReport(int reportId)
        {
            var report = await _majorService.GetReportDetail(reportId);

            return Ok(report);
        }

        // [PUT] /Major/majors/reports/{reportId}
        [HttpPut("reports/{reportId:int}")]
        public async Task<IActionResult> UpdateReport(int reportId)
        {

            await _majorService.ResovleReport(reportId);

            return Ok(new
            {
                // message = "Cập nhật report thành công",
                message = "Update report successfully"
            });
        }

        // [GET] /Major/majors/reportTypes
        [HttpGet("reportTypes")]
        public async Task<IActionResult> GetReportTypes()
        {
            var reportTypes = await _majorService.GetReportTypes();

            return Ok(new
            {
                ReportTypes = reportTypes
            });
        }

        // [GET] /Major/majors/facilityMajorTypes
        [HttpGet("facilityMajorTypes")]
        public async Task<IActionResult> GetFacilityMajorTypes()
        {
            var facilityMajorTypes = await _majorService.GetFacilityMajorTypes();

            return Ok(new
            {
                FacilityMajorTypes = facilityMajorTypes
            });
        }


    }
}
