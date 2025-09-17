using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Helpers;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices;

namespace Architecture_1.API.Controllers.FacilityMajorControllers
{
    [Route("api/Major/services")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class FacilityMajorServiceController : ControllerBase
    {
        private ILogger<FacilityMajorServiceController> _logger;
        private readonly MajorService _majorService;
        private readonly MajorAssignmentService _majorAssignmentService;
        private readonly MajorServicesService _majorServicesService;
        private readonly DateHelpers _dateHelpers;

        public FacilityMajorServiceController(
            ILogger<FacilityMajorServiceController> logger,
            MajorService majorService,
            MajorAssignmentService majorAssignmentService,
            MajorServicesService majorServicesService,
            DateHelpers dateHelpers
            )
        {
            _logger = logger;
            _majorService = majorService;
            _majorAssignmentService = majorAssignmentService;
            _majorServicesService = majorServicesService;
            _dateHelpers = dateHelpers;
        }

        // [GET] /Major/services
        [HttpGet]
        public async Task<IActionResult> GetMajorServices()
        {
            var services = await _majorServicesService.GetAllServices();

            return Ok(new
            {
                Services = services
            });
        }

        // [GET] /Major/services/major-head/{accountId}  
        [HttpGet("major-head/{accountId:int}")]
        public async Task<IActionResult> GetMajorHeadServices(int accountId)
        {
            var services = await _majorServicesService.GetMajorHeadServices(accountId);

            return Ok(new
            {
                Services = services
            });
        }

        // [GET] /Major/services/majors/{majorId}
        [HttpGet("majors/{majorId:int}")]
        public async Task<IActionResult> GetMajorServices(int majorId)
        {
            var services = await _majorServicesService.GetMajorServices(majorId);

            return Ok(new
            {
                Services = services
            });
        }

        // [GET] /Major/services/{serviceId} 
        [HttpGet("{serviceId:int}")]
        public async Task<IActionResult> GetMajorService(int serviceId)
        {
            var service = await _majorServicesService.GetMajorServiceDetail(serviceId);

            return Ok(service);
        }


        // [PUT] /Major/services/{serviceId}
        [HttpPut("{serviceId:int}")]
        public async Task<IActionResult> UpdateMajorService(int serviceId, [FromBody] JToken jsonData)
        {
            dynamic service = jsonData["Service"].ToObject<dynamic>();

            await _majorServicesService.UpdateMajorService(serviceId, service);

            return Ok(new
            {
                // message = "Cập nhật dịch vụ thành công",
                message = "Update major service successfully"
            });
        }

        // [DELETE] /Major/services/{serviceId} 
        [HttpDelete("{serviceId:int}")]
        public async Task<IActionResult> DeleteMajorService(int serviceId)
        {
            await _majorServicesService.DeactivateMajorService(serviceId, true);

            return Ok(new
            {
                // message = "Xóa dịch vụ thành công",
                message = "Delete major service successfully",
            });
        }

        // [GET] /Major/services/{serviceId}/availability  
        [HttpGet("{serviceId:int}/availability")]
        public async Task<IActionResult> GetMajorServiceAvailability(int serviceId)
        {
            var schedules = await _majorServicesService.GetMajorServiceAvailability(serviceId);

            return Ok(new
            {
                Schedules = schedules
            });
        }

        // [GET] /Major/services/{serviceId}/bookable-schedules
        [HttpGet("{serviceId:int}/bookable-schedules")]
        public async Task<IActionResult> GetMajorServiceBookableSchedules(int serviceId)
        {
            var schedules = await _majorServicesService.GetMajorServiceBookableSchedules(serviceId);

            return Ok(new
            {
                Schedules = schedules
            });
        }

        // [POST] /Major/services/major/{majorId}
        [HttpPost("major/{majorId:int}")]
        public async Task<IActionResult> CreateMajorService(int majorId, [FromBody] JToken jsonData)
        {
            dynamic service = jsonData["Service"].ToObject<dynamic>();

            await _majorServicesService.CreateMajorService(majorId, service);

            return Ok(new
            {
                // message = "Tạo dịch vụ thành công",
                message = "Create major service successfully"
            });
        }

        // [POST] /Major/services/{serviceId}/add-availability
        [HttpPost("{serviceId:int}/add-availability")]
        public async Task<IActionResult> AddMajorServiceAvailability(int serviceId, [FromBody] JToken jsonData)
        {
            dynamic availability = jsonData["Availability"].ToObject<dynamic>();

            await _majorServicesService.AddMajorServiceAvailability(serviceId, availability);

            return Ok(new
            {
                // message = "Thêm lịch làm việc thành công",
                message = "Add major service availability successfully"
            });
        }

        // [POST] /Major/services/{serviceId}/delete-availability
        [HttpPost("{serviceId:int}/delete-availability")]
        public async Task<IActionResult> DeleteMajorServiceAvailability(int serviceId, [FromBody] JToken jsonData)
        {
            dynamic availability = jsonData["Availability"].ToObject<dynamic>();

            await _majorServicesService.DeleteMajorServiceAvailability(serviceId, availability);

            return Ok(new
            {
                // message = "Xóa lịch làm việc thành công",
                message = "Delete major service availability successfully"
            });
        }

        // [GET] /Major/services/serviceTypes
        [HttpGet("serviceTypes")]
        public async Task<IActionResult> GetServiceTypes()
        {
            var serviceTypes = await _majorServicesService.GetServiceTypes();

            return Ok(new
            {
                ServiceTypes = serviceTypes
            });
        }


        ///////////////////////////////////////////////////////

        // [POST] /Major/services/test-bookable-schedules
        [HttpPost("test-bookable-schedules")]
        public async Task<IActionResult> TestBookableSchedules([FromBody] JToken jsonData)
        {
            int dateLimit = jsonData["dateLimit"].ToObject<int>();
            dynamic major = jsonData["major"].ToObject<dynamic>();
            dynamic service = jsonData["service"].ToObject<dynamic>();
            DateOnly? major_close_date = major["CloseScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(major["CloseScheduleDate"].ToString())) : null;
            DateOnly? major_open_date = major["OpenScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(major["OpenScheduleDate"].ToString())) : null;
            DateOnly? service_close_date = service["CloseScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(service["CloseScheduleDate"].ToString())) : null;
            DateOnly? service_open_date = service["OpenScheduleDate"] != null ? DateOnly.FromDateTime(DateTime.Parse(service["OpenScheduleDate"].ToString())) : null;





            List<int> daysOfWeeks = jsonData["availability"].ToObject<List<int>>();

            if (major_close_date != null && major_open_date != null && service_close_date != null && service_open_date != null)
            {
                if (!(major_open_date <= service_close_date || service_open_date < major_close_date))
                {
                    if (major_close_date < service_close_date)
                    {
                        service_close_date = major_close_date;
                    }
                    else
                    {
                        major_close_date = service_close_date;
                    }

                    if (major_open_date < service_open_date)
                    {
                        major_open_date = service_open_date;
                    }
                    else
                    {
                        service_open_date = major_open_date;
                    }
                }
            }
            

            List<string> majorAvailableDates = _majorServicesService.GetValidDates(dateLimit, major_close_date, major_open_date, daysOfWeeks);
            List<string> serviceAvailableDates = _majorServicesService.GetValidDates(dateLimit, service_close_date, service_open_date, daysOfWeeks);



            if (major_close_date == null && service_close_date != null)
            {
                majorAvailableDates = serviceAvailableDates;
            }
            else if (major_close_date != null && service_close_date == null)
            {
                serviceAvailableDates = majorAvailableDates;
            }


            List<string> availableDates = majorAvailableDates.Intersect(serviceAvailableDates).ToList();

            if (availableDates.Count > dateLimit)
            {
                availableDates = availableDates.GetRange(0, dateLimit);
            }
            else if (availableDates.Count < dateLimit && major_open_date != null && service_open_date != null)
            {
                DateOnly? date = DateOnly.Parse(majorAvailableDates[majorAvailableDates.Count - 1]) > DateOnly.Parse(serviceAvailableDates[serviceAvailableDates.Count - 1]) ? DateOnly.Parse(majorAvailableDates[majorAvailableDates.Count - 1]) : DateOnly.Parse(serviceAvailableDates[serviceAvailableDates.Count - 1]);

                if(major_close_date > service_open_date){
                    date = service_open_date;
                }else if (service_close_date > major_open_date){
                    date = major_open_date;
                }else if (major_open_date > service_open_date)
                {
                    if (DateOnly.Parse(majorAvailableDates[majorAvailableDates.Count - 1]) >= major_open_date)
                    {
                        date = major_open_date;
                    }
                }else if(service_open_date > major_open_date)
                {
                    if (DateOnly.Parse(serviceAvailableDates[serviceAvailableDates.Count - 1]) >= service_open_date)
                    {
                        date = service_open_date;
                    }
                }

                if(availableDates.Contains(date?.ToString("yyyy-MM-dd")))
                {
                    date = date?.AddDays(1);
                }

                while (availableDates.Count < dateLimit)
                {
                    int dayOfWeek = ((int)date?.DayOfWeek) + 1;
                    if (daysOfWeeks.Contains(dayOfWeek) && !_dateHelpers.IsDateInRange(date.ToString(), major_close_date.ToString(), major_open_date?.AddDays(-1).ToString()) && !_dateHelpers.IsDateInRange(date.ToString(), service_close_date.ToString(), service_open_date?.AddDays(-1).ToString()) && !availableDates.Contains(date?.ToString("yyyy-MM-dd")))
                    {
                        Console.WriteLine(true);
                        availableDates.Add(date?.ToString("yyyy-MM-dd"));
                    }
                    date = date?.AddDays(1);
                }
            }

            return Ok(new
            {
                
                schedule = new
                {
                    major_close_date = major_close_date,
                    major_open_date = major_open_date,
                    service_close_date = service_close_date,
                    service_open_date = service_open_date
                },
                availableDates = availableDates,
                daysOfWeeks,
                majorAvailableDates,
                serviceAvailableDates
            });
        }
    }
}
