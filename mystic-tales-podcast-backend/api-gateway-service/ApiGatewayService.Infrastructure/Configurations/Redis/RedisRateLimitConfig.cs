using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisRateLimitConfigModel
    {
        public int DefaultLimit { get; set; } = 1000;
        public int WindowMinutes { get; set; } = 1;
        public bool EnableRateLimit { get; set; } = true;
        public string RateLimitKeyPrefix { get; set; } = "ratelimit:";
        public int BurstLimit { get; set; } = 1500;
    }

    public class RedisRateLimitConfig : IRedisRateLimitConfig
    {
        public int DefaultLimit { get; set; } = 1000;
        public int WindowMinutes { get; set; } = 1;
        public bool EnableRateLimit { get; set; } = true;
        public string RateLimitKeyPrefix { get; set; } = "ratelimit:";
        public int BurstLimit { get; set; } = 1500;

        public RedisRateLimitConfig(IConfiguration configuration)
        {
            var rateLimitConfig = configuration.GetSection("Infrastructure:Redis:RateLimit")
                .Get<RedisRateLimitConfigModel>();
            if (rateLimitConfig != null)
            {
                DefaultLimit = rateLimitConfig.DefaultLimit;
                WindowMinutes = rateLimitConfig.WindowMinutes;
                EnableRateLimit = rateLimitConfig.EnableRateLimit;
                RateLimitKeyPrefix = rateLimitConfig.RateLimitKeyPrefix;
                BurstLimit = rateLimitConfig.BurstLimit;
            }
        }
    }
}
