using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Redis
{
    public class RedisRateLimitConfigModel
    {
        public string KeyPrefix { get; set; }
        public int WindowSeconds { get; set; }
        public int MaxRequests { get; set; }
    }
    public class RedisRateLimitConfig : IRedisRateLimitConfig
    {
        public string KeyPrefix { get; set; }
        public int WindowSeconds { get; set; }
        public int MaxRequests { get; set; }

        public RedisRateLimitConfig(IConfiguration configuration)
        {
            var rateLimitConfig = configuration.GetSection("Infrastructure:Redis:RateLimit").Get<RedisRateLimitConfigModel>();
            KeyPrefix = rateLimitConfig.KeyPrefix;
            WindowSeconds = rateLimitConfig.WindowSeconds;
            MaxRequests = rateLimitConfig.MaxRequests;
        }
    }
} 