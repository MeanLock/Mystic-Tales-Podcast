using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using TransactionService.API.Filters.ExceptionFilters;
using TransactionService.BusinessLogic.DTOs.Cache;
using TransactionService.BusinessLogic.Models.CrossService;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using TransactionService.BusinessLogic.Services.DbServices.TransactionServices;

namespace TransactionService.API.Controllers.BaseControllers
{
    [Route("api/subscription-transactions")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class SubscriptionTransactionController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly PodcastSubscriptionService _podcastSubscriptionService;
        private readonly MemberSubscriptionService _memberSubscriptionService;

        public SubscriptionTransactionController(
            GenericQueryService genericQueryService, 
            HttpServiceQueryClient httpServiceQueryClient,
            PodcastSubscriptionService podcastSubscriptionService,
            MemberSubscriptionService memberSubscriptionService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _podcastSubscriptionService = podcastSubscriptionService;
            _memberSubscriptionService = memberSubscriptionService;
        }
        [HttpGet("podcast-subscription-registrations/{PodcastSubscriptionRegistrationId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetPodcastSubscriptionRegistrationById([FromRoute] Guid PodcastSubscriptionRegistrationId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _podcastSubscriptionService.GetPodcastSubscriptionRegistration(accountId, PodcastSubscriptionRegistrationId);
            if (isValid == null)
            {
                return Forbid($"You are not authorize to see the transaction of Podcast Subscription Registration Id: {PodcastSubscriptionRegistrationId}");
            }
            //Console.WriteLine(isValid.ToString());
            var result = await _podcastSubscriptionService.GetPodcastSubscriptionTransactions(PodcastSubscriptionRegistrationId);
            //if(result == null || result.Count == 0)
            //{
            //    return NotFound($"No transaction found for Podcast Subscription Registration Id: {PodcastSubscriptionRegistrationId}");
            //}
            return Ok(new
            {
                PodcastSubscriptionTransactionList = result
            });
        }
        [HttpGet("member-subscription-registrations/{MemberSubscriptionRegistrationId}")]
        public async Task<IActionResult> GetMemberSubscriptionRegistrationById([FromRoute] Guid MemberSubscriptionRegistrationId)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;
            var accountId = account.Id;

            var isValid = await _memberSubscriptionService.GetMemberSubscriptionRegistration(accountId, MemberSubscriptionRegistrationId);
            if (isValid == null)
            {
                return Forbid($"You are not authorize to see the transaction of Member Subscription Registration Id: {MemberSubscriptionRegistrationId}");
            }
            var result = await _memberSubscriptionService.GetMemberSubscriptionTransactions(MemberSubscriptionRegistrationId);
            //if (result == null || result.Count == 0)
            //{
            //    return NotFound($"No transaction found for Member Subscription Registration Id: {MemberSubscriptionRegistrationId}");
            //}
            return Ok(new
            {
                MemberSubscriptionTransactionList = result
            });
        }
    }
}
