using Microsoft.AspNetCore.Mvc;
using SystemConfigurationService.API.Filters.ExceptionFilters;
using SystemConfigurationService.BusinessLogic.Models.CrossService;
using SystemConfigurationService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SystemConfigurationService.API.Controllers.BaseControllers
{
    [Route("api/system-configs")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class SystemConfigController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public SystemConfigController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

    }
}
