using Microsoft.AspNetCore.Mvc;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/episodes")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class EpisodeController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public EpisodeController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        
    }
}
