using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Controllers.FacilityMajorControllers;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices;
using Architecture_1.BusinessLogic.Services.DbServices.RequestServices;

namespace Architecture_1.API.Controllers.RequestControllers
{
    [Route("api/Request/service")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class ServiceRequestController : ControllerBase
    {
        private ILogger<ServiceRequestController> _logger;
        private readonly ServiceRequestService _serviceRequestService;

        public ServiceRequestController(ILogger<ServiceRequestController> logger,
            ServiceRequestService serviceRequestService
            )
        {
            _logger = logger;
            _serviceRequestService = serviceRequestService;
        }
        

        // [GET] /Request/service/major-head/{accountId}
        [HttpGet("major-head/{accountId:int}")]
        public async Task<IActionResult> GetMajorHeadServiceRequests(int accountId)
        {
            var serviceRequests = await _serviceRequestService.GetMajorHeadServiceRequests(accountId);

            return Ok(new
            {
                ServiceRequests = serviceRequests
            });
        }

        // [GET] /Request/service/majors/{majorId}
        [HttpGet("majors/{majorId:int}")]
        public async Task<IActionResult> GetMajorServiceRequests(int majorId)
        {
            var serviceRequests = await _serviceRequestService.GetMajorServiceRequests(majorId);

            return Ok(new
            {
                ServiceRequests = serviceRequests
            });
        }

        // [GET] /Request/service/assignee/{accountId}
        [HttpGet("assignee/{accountId:int}")]
        public async Task<IActionResult> GetAssigneeServiceRequests(int accountId)
        {
            var serviceRequests = await _serviceRequestService.GetAssigneeServiceRequests(accountId);

            return Ok(new
            {
                ServiceRequests = serviceRequests
            });
        }

        // [GET] /Request/service/assignee/{accountId}/majors/{majorId}
        [HttpGet("assignee/{accountId:int}/majors/{majorId:int}")]
        public async Task<IActionResult> GetAssigneeMajorServiceRequests(int accountId, int majorId)
        {
            var serviceRequests = await _serviceRequestService.GetAssigneeMajorServiceRequests(accountId, majorId);

            return Ok(new
            {
                ServiceRequests = serviceRequests
            });
        }

        // [GET] /Request/service/requester/{accountId}
        [HttpGet("requester/{accountId:int}")]
        public async Task<IActionResult> GetRequesterMajorServiceRequests(int accountId)
        {
            var serviceRequests = await _serviceRequestService.GetRequesterServiceRequests(accountId);

            return Ok(new
            {
                ServiceRequests = serviceRequests
            });
        }

        // [GET] /Request/service/{requestId}
        [HttpGet("{requestId:int}")]
        public async Task<IActionResult> GetServiceRequest(int requestId)
        {
            var serviceRequest = await _serviceRequestService.GetServiceRequestDetail(requestId);

            return Ok(serviceRequest);
        }

        // [PUT] /Request/service/{requestId}?Action = {action} (***CHƯA LÀM XONG)
        [HttpPut("{requestId:int}")]
        public async Task<IActionResult> UpdateServiceRequest(int requestId, [FromBody] JToken jsonData, [FromQuery] string action)
        {
            dynamic updateInformations = jsonData.ToObject<dynamic>();

            await _serviceRequestService.UpdateServiceRequest(requestId, action, updateInformations);

            return Ok(new
            {
                // message = "Cập nhật yêu cầu dịch vụ thành công",
                message = "Update service request successfully"
            });
        }

        // [GET] /Request/service/major/{majorId}/assignable-assignee
        [HttpGet("major/{majorId:int}/assignable-assignee")]
        public async Task<IActionResult> GetAssignableAssignee(int majorId)
        {
            var assignableAssignee = await _serviceRequestService.GetAssignableAssignee(majorId);

            return Ok(new
            {
                Accounts = assignableAssignee
            });
        }

        // [POST] /Request/service
        [HttpPost]
        public async Task<IActionResult> CreateServiceRequest([FromBody] JToken jsonData)
        {
            dynamic serviceRequest = jsonData["ServiceRequest"].ToObject<dynamic>();

            await _serviceRequestService.CreateServiceRequest(serviceRequest);

            return Ok(new
            {
                // message = "Tạo yêu cầu dịch vụ thành công",
                message = "Create service request successfully"
            });
        }

        // [GET] /Request/service/requestStatuses
        [HttpGet("requestStatuses")]
        public async Task<IActionResult> GetRequestStatuses()
        {
            var requestStatuses = await _serviceRequestService.GetRequestStatuses();

            return Ok(new
            {
                RequestStatuses = requestStatuses
            });
        }

        
    }
}
