using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using PodcastService.API.Authorizations.Requirements;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.DataAccess.Entities;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.Infrastructure.Services.Redis;

namespace PodcastService.API.Authorizations.Handlers
{
    public class AccountOptionalAccessHandler : AuthorizationHandler<AccountOptionalAccessRequirement>
    {
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly RedisSharedCacheService _redisSharedCacheService;
        public AccountOptionalAccessHandler(
            HttpServiceQueryClient httpServiceQueryClient,
            RedisSharedCacheService redisSharedCacheService
            )
        {
            _httpServiceQueryClient = httpServiceQueryClient;
            _redisSharedCacheService = redisSharedCacheService;
        }

        public async Task<JObject> GetAccountById(int accountId)
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
                            id = accountId
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
                            "UpdatedAt"
                        }
                    }
                }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("PodcastService", batchRequest);
            return result.Results["account"] as JObject;
        }

        public async Task<AccountStatusCache> GetAccountStatusCacheById(int accountId)
        {
            var cacheKey = $"account:status:{accountId}";
            var cachedData = await _redisSharedCacheService.KeyGetAsync<AccountStatusCache>(cacheKey);

            return cachedData;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountOptionalAccessRequirement requirement)
        {
            // NẾU KHÔNG CÓ TOKEN THÌ VẪN CHO QUA
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                Console.WriteLine("No authenticated user found in context. Skipping account validation.");
                context.Succeed(requirement);
                return;
            }

            string userId = context.User.FindFirst("id")?.Value;
            string roleId = context.User.FindFirst("role_id")?.Value;
            Console.WriteLine($"000000000000000000000000000000000000000000000000000000000UserId: {userId}, RoleId: {roleId}");
            if (roleId == null || !int.TryParse(roleId, out _))
            {
                context.Fail();
                return;
            }
            if (userId == null) { context.Fail(); return; }

            // var account = await _accountRepository.FindByIdAsync(int.Parse(userId));
            // var account = await _accountGenericRepository.FindByIdAsync(int.Parse(userId), a => );
            var account = await GetAccountStatusCacheById(int.Parse(userId));

            if (account == null)
            {
                Console.WriteLine($"Fetching account status from database for account id: {userId}");
                account = (await GetAccountById(int.Parse(userId)))?.ToObject<AccountStatusCache>();
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
