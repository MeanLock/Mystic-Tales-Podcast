using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using SystemConfigurationService.API.Authorizations.Requirements;
using SystemConfigurationService.BusinessLogic.DTOs.Cache;
using SystemConfigurationService.BusinessLogic.Models.CrossService;
using SystemConfigurationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using SystemConfigurationService.DataAccess.Repositories.interfaces;
using SystemConfigurationService.Infrastructure.Services.Redis;

namespace SystemConfigurationService.API.Authorizations.Handlers
{
    public class AccountPodcasterAccessHandler : AuthorizationHandler<AccountPodcasterAccessRequirement>
    {
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly RedisSharedCacheService _redisSharedCacheService;
        public AccountPodcasterAccessHandler(
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
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);
            return result.Results["account"] as JObject;
        }

        public async Task<AccountStatusCache> GetAccountStatusCacheById(int accountId)
        {
            var cacheKey = $"account:status:{accountId}";
            var cachedData = await _redisSharedCacheService.KeyGetAsync<AccountStatusCache>(cacheKey);

            return cachedData;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountPodcasterAccessRequirement requirement)
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
            else if (account.HasVerifiedPodcasterProfile == false)
            {
                Console.WriteLine($"0000000000000000000000000000000000000000000000000000000Account has not verified podcaster profile: {account.HasVerifiedPodcasterProfile}");
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
