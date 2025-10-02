using Microsoft.AspNetCore.Mvc;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace ModerationService.API.Controllers.BaseControllers
{
    [Route("api/dmca-accusations")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class DMCAAccusationController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public DMCAAccusationController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

       
    }
}
