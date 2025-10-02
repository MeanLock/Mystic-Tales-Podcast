using Microsoft.AspNetCore.Mvc;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SubscriptionService.API.Controllers.BaseControllers
{
    [Route("api/podcast-subscriptions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class PodcastSubscriptionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public PodcastSubscriptionController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        
    }
}
