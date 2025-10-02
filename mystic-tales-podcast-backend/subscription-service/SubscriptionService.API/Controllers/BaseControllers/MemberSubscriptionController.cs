using Microsoft.AspNetCore.Mvc;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SubscriptionService.API.Controllers.BaseControllers
{
    [Route("api/member-subscriptions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class MemberSubscriptionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public MemberSubscriptionController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        
    }
}
