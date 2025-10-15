using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModerationService.API.Filters.ExceptionFilters;
using ModerationService.BusinessLogic.Models.CrossService;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace ModerationService.API.Controllers.BaseControllers
{
    [Route("api/episode-reports")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class EpisodeReportController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public EpisodeReportController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }


    }
}
