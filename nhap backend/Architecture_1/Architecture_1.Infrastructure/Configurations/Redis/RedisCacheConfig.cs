using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.AWS.interfaces;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Redis
{
    public class RedisCacheConfigModel
    {
        public string KeyPrefix { get; set; }
        public int ExpirySeconds { get; set; }
        public int SlidingExpirationSeconds { get; set; }
    }
    public class RedisCacheConfig : IRedisCacheConfig
    {
        public string KeyPrefix { get; set; }
        public int ExpirySeconds { get; set; }
        public int SlidingExpirationSeconds { get; set; }

        public RedisCacheConfig(IConfiguration configuration)
        {
            var cacheConfig = configuration.GetSection("Infrastructure:Redis:Cache").Get<RedisCacheConfigModel>();
            KeyPrefix = cacheConfig.KeyPrefix;
            ExpirySeconds = cacheConfig.ExpirySeconds;
            SlidingExpirationSeconds = cacheConfig.SlidingExpirationSeconds;
        }
    }

}
