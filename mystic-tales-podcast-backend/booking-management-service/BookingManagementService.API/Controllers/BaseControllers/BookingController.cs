using Microsoft.AspNetCore.Mvc;
using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace BookingManagementService.API.Controllers.BaseControllers
{
    [Route("api/bookings")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class BookingController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public BookingController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

    }
}
