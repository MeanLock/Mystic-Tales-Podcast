using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using PodcastService.API.Authorizations.Requirements;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.Enums.Account;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.Infrastructure.Services.Redis;

namespace PodcastService.API.Authorizations.Handlers
{
    public class AdminStaffBasicAccessOrCustomerPodcasterAccessHandler : AuthorizationHandler<AdminStaffBasicAccessOrCustomerPodcasterAccessRequirement>
    {
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly RedisSharedCacheService _redisSharedCacheService;

        public AdminStaffBasicAccessOrCustomerPodcasterAccessHandler(
            HttpServiceQueryClient httpServiceQueryClient,
            RedisSharedCacheService redisSharedCacheService)
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
            accountStatusCache.PodcasterProfileVerifiedAt = podcasterProfile?["VerifiedAt"]?.ToObject<DateTime?>();
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

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminStaffBasicAccessOrCustomerPodcasterAccessRequirement requirement)
        {
            string userId = context.User.FindFirst("id")?.Value;
            string roleId = context.User.FindFirst("role_id")?.Value;
            Console.WriteLine($"AdminStaffBasicAccessOrCustomerPodcasterAccessHandler - UserId: {userId}, RoleId: {roleId}");

            if (roleId == null || !int.TryParse(roleId, out int roleIdInt))
            {
                Console.WriteLine($"Invalid role_id in token");
                context.Fail();
                return;
            }

            if (userId == null)
            {
                Console.WriteLine($"UserId is null");
                context.Fail();
                return;
            }

            var account = await GetAccountStatusCacheById(int.Parse(userId));

            if (account == null)
            {
                Console.WriteLine($"Fetching account status from database for account id: {userId}");
                account = await QueryAccountStatusCacheById(int.Parse(userId));
                if (account == null)
                {
                    Console.WriteLine($"Account not found: {userId}");
                    context.Fail();
                    return;
                }
                else
                {
                    Console.WriteLine($"Caching account status to redis for account id: {userId}");
                    await _redisSharedCacheService.KeySetAsync<AccountStatusCache>($"account:status:{account.Id}", account, null);
                }
            }

            Console.WriteLine($"Account fetched: Id={account.Id}, RoleId={account.RoleId}, IsVerified={account.IsVerified}, DeactivatedAt={account.DeactivatedAt}, HasVerifiedPodcasterProfile={account.HasVerifiedPodcasterProfile}");

            // Kiểm tra role mismatch
            if (account.RoleId != roleIdInt)
            {
                Console.WriteLine($"Account role mismatch: {account.RoleId} != {roleIdInt}");
                context.Fail();
                return;
            }

            // Kiểm tra account verified
            if (account.IsVerified == false)
            {
                Console.WriteLine($"Account is not verified: {account.IsVerified}");
                context.Fail();
                return;
            }

            // Kiểm tra deactivated
            if (account.DeactivatedAt != null)
            {
                Console.WriteLine($"Account is deactivated: {account.DeactivatedAt}");
                context.Fail();
                return;
            }

            // Logic phân nhánh theo role
            // RoleId: 1 = Customer, 2 = Staff, 3 = Admin
            if (roleIdInt == (int)RoleEnum.Staff || roleIdInt == (int)RoleEnum.Admin) // Admin hoặc Staff
            {
                Console.WriteLine($"Admin/Staff access - basic check passed");
                // Admin/Staff chỉ cần check basic (đã check ở trên)
                // Lưu account vào HttpContext
                if (context.Resource is HttpContext httpContext)
                {
                    httpContext.Items["LoggedInAccount"] = account;
                }
                else if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext authContext)
                {
                    authContext.HttpContext.Items["LoggedInAccount"] = account;
                }
                context.Succeed(requirement);
            }
            else if (roleIdInt == (int)RoleEnum.Customer) // Customer
            {
                // Customer cần check thêm HasVerifiedPodcasterProfile
                if (account.HasVerifiedPodcasterProfile == false)
                {
                    Console.WriteLine($"Customer does not have verified podcaster profile");
                    context.Fail();
                    return;
                }

                Console.WriteLine($"Customer podcaster access - all checks passed");
                // Lưu account vào HttpContext
                if (context.Resource is HttpContext httpContext)
                {
                    httpContext.Items["LoggedInAccount"] = account;
                }
                else if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext authContext)
                {
                    authContext.HttpContext.Items["LoggedInAccount"] = account;
                }
                context.Succeed(requirement);
            }
            else
            {
                Console.WriteLine($"Invalid role: {roleIdInt}");
                context.Fail();
            }
        }
    }
}