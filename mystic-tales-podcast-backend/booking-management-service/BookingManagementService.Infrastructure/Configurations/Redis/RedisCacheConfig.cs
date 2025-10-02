using Microsoft.Extensions.Configuration;
using BookingManagementService.Infrastructure.Configurations.AWS.interfaces;
using BookingManagementService.Infrastructure.Configurations.Redis.interfaces;

namespace BookingManagementService.Infrastructure.Configurations.Redis
{
    public class RedisCacheConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }
        public int SlidingExpirationSeconds { get; set; }
    }
    public class RedisCacheConfig : IRedisCacheConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }
        public int SlidingExpirationSeconds { get; set; }

        public RedisCacheConfig(IConfiguration configuration)
        {
            var cacheConfig = configuration.GetSection("Infrastructure:Redis:Cache").Get<RedisCacheConfigModel>();
            if (cacheConfig != null)
            {
                KeyPrefix = cacheConfig.KeyPrefix;
                ExpirySeconds = cacheConfig.ExpirySeconds;
                SlidingExpirationSeconds = cacheConfig.SlidingExpirationSeconds;
            }
        }
    }

}
