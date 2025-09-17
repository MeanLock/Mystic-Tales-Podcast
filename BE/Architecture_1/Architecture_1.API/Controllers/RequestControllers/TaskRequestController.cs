using System.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.DbServices.RequestServices;

namespace Architecture_1.API.Controllers.RequestControllers
{
    [Route("api/Request/task")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class TaskRequestController : ControllerBase
    {
        private ILogger<TaskRequestController> _logger;
        private readonly TaskRequestService _taskRequestService;

        public TaskRequestController(ILogger<TaskRequestController> logger,
            TaskRequestService taskRequestService
            )
        {
            _logger = logger;
            _taskRequestService = taskRequestService;
        }

        // [GET] /Request/task
        [HttpGet]
        public async Task<IActionResult> GetTaskRequests()
        {
            var taskRequests = await _taskRequestService.GetTaskRequests();

            return Ok(new
            {
                TaskRequests = taskRequests
            });
        }

        // [POST] /Request/task 
        [HttpPost]
        public async Task<IActionResult> CreateTaskRequest([FromBody] JToken jsonData)
        {
            dynamic taskRequest = jsonData["TaskRequest"].ToObject<dynamic>();

            await _taskRequestService.CreateTaskRequest(taskRequest);

            return Ok(new
            {
                // message = "Tạo yêu cầu công việc thành công",
                message = "Create task request successfully"
            });
        }

        // [GET] /Request/task/major-head/{accountId}
        [HttpGet("major-head/{accountId:int}")]
        public async Task<IActionResult> GetMajorHeadTaskRequests(int accountId)
        {
            var taskRequests = await _taskRequestService.GetMajorHeadTaskRequests(accountId);

            return Ok(new
            {
                TaskRequests = taskRequests
            });
        }

        // [GET] /Request/task/majors/{majorId}
        [HttpGet("majors/{majorId:int}")]
        public async Task<IActionResult> GetTaskRequestsByMajorId(int majorId)
        {
            var taskRequests = await _taskRequestService.GetTaskRequestsByMajorId(majorId);

            return Ok(new
            {
                TaskRequests = taskRequests
            });
        }

        // [GET] /Request/task/{requestId}
        [HttpGet("{requestId:int}")]
        public async Task<IActionResult> GetTaskRequest(int requestId)
        {
            var taskRequest = await _taskRequestService.GetTaskRequestDetail(requestId);

            return Ok(taskRequest);
        }

        // [PUT] /Request/task/{taskId}?Action = {action}
        [HttpPut("{taskId:int}")]
        public async Task<IActionResult> UpdateTaskRequest(int taskId, [FromBody] JToken jsonData, [FromQuery] string action)
        {
            string cancelReason = jsonData["CancelReason"]?.ToString();

            dynamic updateInfo = new ExpandoObject();
            updateInfo.CancelReason = cancelReason;

            await _taskRequestService.UpdateRequestStatus(taskId, action, updateInfo);

            return Ok(new
            {
                // message = "Cập nhật yêu cầu công việc thành công",
                message = "Update task request successfully"
            });
        }

    }
}
