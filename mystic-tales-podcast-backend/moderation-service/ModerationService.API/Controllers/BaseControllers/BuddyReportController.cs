using Microsoft.AspNetCore.Mvc;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace ModerationService.API.Controllers.BaseControllers
{
    [Route("api/buddy-reports")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class BuddyReportController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public BuddyReportController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        
    }
}
