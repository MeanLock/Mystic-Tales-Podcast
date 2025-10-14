using Microsoft.Extensions.Configuration;
using SagaOrchestratorService.Infrastructure.Configurations.Redis.interfaces;

namespace SagaOrchestratorService.Infrastructure.Configurations.Redis
{
    public class RedisRateLimitConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int WindowSeconds { get; set; }
        public int MaxRequests { get; set; }
    }
    public class RedisRateLimitConfig : IRedisRateLimitConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int WindowSeconds { get; set; }
        public int MaxRequests { get; set; }

        public RedisRateLimitConfig(IConfiguration configuration)
        {
            var rateLimitConfig = configuration.GetSection("Infrastructure:Redis:RateLimit").Get<RedisRateLimitConfigModel>();
            if (rateLimitConfig != null)
            {
                KeyPrefix = rateLimitConfig.KeyPrefix;
                WindowSeconds = rateLimitConfig.WindowSeconds;
                MaxRequests = rateLimitConfig.MaxRequests;
            }
        }
    }
} 