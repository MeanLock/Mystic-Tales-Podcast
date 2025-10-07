using Microsoft.Extensions.Configuration;
using UserService.Infrastructure.Configurations.AWS.interfaces;
using UserService.Infrastructure.Configurations.Redis.interfaces;

namespace UserService.Infrastructure.Configurations.Redis
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
