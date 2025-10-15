using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.API.Filters.ExceptionFilters;
using SubscriptionService.BusinessLogic.DTOs.Cache;
using SubscriptionService.BusinessLogic.Models.CrossService;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SubscriptionService.API.Controllers.BaseControllers
{
    [Route("api/member-subscriptions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class MemberSubscriptionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public MemberSubscriptionController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

        [HttpGet("test-get-account-status")]
        [Authorize(Policy = "Customer.NoViolationAccess")]
        public async Task<IActionResult> TestGetAccountStatus()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            return Ok(new
            {
                Account = account,
                Message = $"Hello, your account ID is {account.Id}, RoleId is {account.RoleId}, ViolationLevel is {account.ViolationLevel}, ViolationPoint is {account.ViolationPoint}, IsVerified is {account.IsVerified}, DeactivatedAt is {account.DeactivatedAt}, LastViolationLevelChanged is {account.LastViolationLevelChanged}, LastViolationPointChanged is {account.LastViolationPointChanged}"
            });
        }


    }
}
