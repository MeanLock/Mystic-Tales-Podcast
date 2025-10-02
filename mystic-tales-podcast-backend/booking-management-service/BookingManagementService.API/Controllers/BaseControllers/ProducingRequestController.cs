using Microsoft.AspNetCore.Mvc;
using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace BookingManagementService.API.Controllers.BaseControllers
{
    [Route("api/producing-requests")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class ProducingRequestController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public ProducingRequestController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        
    }
}
