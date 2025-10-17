using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using UserService.API.Authorizations.Requirements;
using UserService.BusinessLogic.DTOs.Cache;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.DataAccess.Repositories.interfaces;
using UserService.Infrastructure.Services.Redis;

namespace UserService.API.Authorizations.Handlers
{
    public class AccountNoViolationAccessHandler : AuthorizationHandler<AccountNoViolationAccessRequirement>
    {
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly RedisSharedCacheService _redisSharedCacheService;
        public AccountNoViolationAccessHandler(
            HttpServiceQueryClient httpServiceQueryClient,
            RedisSharedCacheService redisSharedCacheService
            )
        {
            _httpServiceQueryClient = httpServiceQueryClient;
            _redisSharedCacheService = redisSharedCacheService;
        }

        public async Task<AccountStatusCache> QueryAccountStatusCacheById(int accountId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "account",
                        QueryType = "findbyid",
                        EntityType = "Account",
                        Parameters = JObject.FromObject(new
                        {
                            id = accountId,
                            include = "PodcasterProfile" 
                        }),
                        Fields = new[] {
                            "Id",
                            "Email",
                            "Password",
                            "RoleId",
                            "FullName",
                            "Dob",
                            "Gender",
                            "Address",
                            "Phone",
                            "Balance",
                            "MainImageFileKey",
                            "IsVerified",
                            "GoogleId",
                            "VerifyCode",
                            "PodcastListenSlot",
                            "ViolationPoint",
                            "ViolationLevel",
                            "LastViolationPointChanged",
                            "LastViolationLevelChanged",
                            "LastPodcastListenSlotChanged",
                            "DeactivatedAt",
                            "CreatedAt",
                            "UpdatedAt",
                            "PodcasterProfile",
                        }
                    }
                }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            if (result.Results["account"] == null) return null;


            var accountStatusCache = (result.Results["account"] as JObject).ToObject<AccountStatusCache>();
            var podcasterProfile = result.Results["account"]["PodcasterProfile"] as JObject;
            accountStatusCache.PodcasterProfileName = podcasterProfile?["Name"]?.ToObject<string>();
            accountStatusCache.PodcasterProfileIsVerified = podcasterProfile?["IsVerified"]?.ToObject<bool>();
            accountStatusCache.HasVerifiedPodcasterProfile = accountStatusCache.RoleId == 1 && podcasterProfile != null && podcasterProfile["IsVerified"]?.ToObject<bool>() == true ? true : false;
            Console.WriteLine($"Queried Account: Id={accountStatusCache.Id}, RoleId={accountStatusCache.RoleId}, IsVerified={accountStatusCache.IsVerified}, DeactivatedAt={accountStatusCache.DeactivatedAt}, HasVerifiedPodcasterProfile={accountStatusCache.HasVerifiedPodcasterProfile}");
            return accountStatusCache;
        }

        public async Task<AccountStatusCache> GetAccountStatusCacheById(int accountId)
        {
            var cacheKey = $"account:status:{accountId}";
            var cachedData = await _redisSharedCacheService.KeyGetAsync<AccountStatusCache>(cacheKey);

            return cachedData;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountNoViolationAccessRequirement requirement)
        {
            string userId = context.User.FindFirst("id")?.Value;
            string roleId = context.User.FindFirst("role_id")?.Value;
            Console.WriteLine($"000000000000000000000000000000000000000000000000000000000UserId: {userId}, RoleId: {roleId}");
            if (roleId == null || !int.TryParse(roleId, out _))
            {
                context.Fail();
                return;
            }
            if (userId == null) { context.Fail(); return; }

            var account = await GetAccountStatusCacheById(int.Parse(userId));

            if (account == null)
            {
                Console.WriteLine($"Fetching account status from database for account id: {userId}");
                account = await QueryAccountStatusCacheById(int.Parse(userId));
                if (account == null)
                {
                    Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account not found: {userId}");
                    context.Fail();
                    return;
                }
                else
                {
                    Console.WriteLine($"Caching account status to redis for account id: {userId}");
                    await _redisSharedCacheService.KeySetAsync<AccountStatusCache>($"account:status:{account.Id}", account, null);
                }
            }

            Console.WriteLine($"Account fetched: Id={account.Id}, RoleId={account.RoleId}, IsVerified={account.IsVerified}, DeactivatedAt={account.DeactivatedAt}");

            if (account.RoleId != int.Parse(roleId))
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account role mismatch: {account.RoleId} != {roleId}");
                context.Fail();
            }
            else if (account.IsVerified == false)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account is not verified: {account.IsVerified}");
                context.Fail();
            }
            else if (account.DeactivatedAt != null)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account is deactivated: {account.DeactivatedAt}");
                context.Fail();
            }
            else if (account.ViolationLevel > 0)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account has violations: {account.ViolationLevel}");
                context.Fail();
            }
            else
            {
                // Lưu account vào HttpContext.Items
                if (context.Resource is HttpContext httpContext)
                {
                    httpContext.Items["LoggedInAccount"] = account;
                }
                else if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext authContext)
                {
                    authContext.HttpContext.Items["LoggedInAccount"] = account;
                }
                // Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account: {account.SurveyTopicFavorites.Count}");
                context.Succeed(requirement);
            }
        }
    }
}
