using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.Infrastructure.Services.Redis;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.Models.CrossService;
using Newtonsoft.Json.Linq;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace ModerationService.BusinessLogic.Services.DbServices.MiscServices
{
    public class AccountCachingService
    {
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly RedisSharedCacheService _redisSharedCacheService;
        private readonly IAccountConfig _accountConfig;



        public AccountCachingService(
            HttpServiceQueryClient httpServiceQueryClient,
            RedisSharedCacheService redisSharedCacheService,
            IAccountConfig accountConfig
            )
        {
            _httpServiceQueryClient = httpServiceQueryClient;
            _redisSharedCacheService = redisSharedCacheService;
            _accountConfig = accountConfig;
        }

        public async Task<AccountStatusCache> GetAccountStatusCacheById(int accountId)
        {
            var cacheKey = $"account:status:{accountId}";
            var cachedData = await _redisSharedCacheService.KeyGetAsync<AccountStatusCache>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }
            else
            {
                var accountStatus = await QueryAccountStatusCacheById(accountId);
                if (accountStatus != null)
                {
                    await _redisSharedCacheService.KeySetAsync(cacheKey, accountStatus, TimeSpan.FromSeconds(_accountConfig.AccountStatusCacheExpirySeconds));
                }
                return accountStatus;
            }
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
            accountStatusCache.PodcasterProfileIsVerified = podcasterProfile?["IsVerified"]?.ToObject<bool?>();
            accountStatusCache.PodcasterProfileIsBuddy = podcasterProfile?["IsBuddy"]?.ToObject<bool>() ?? false;
            accountStatusCache.PodcasterProfileVerifiedAt = podcasterProfile?["VerifiedAt"]?.ToObject<DateTime?>();
            accountStatusCache.HasVerifiedPodcasterProfile = accountStatusCache.RoleId == 1 && podcasterProfile != null && podcasterProfile["IsVerified"]?.ToObject<bool?>() == true ? true : false;
            return accountStatusCache;
        }


    }
}
