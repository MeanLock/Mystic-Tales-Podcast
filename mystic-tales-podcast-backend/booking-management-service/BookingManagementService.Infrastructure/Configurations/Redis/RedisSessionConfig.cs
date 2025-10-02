using Microsoft.Extensions.Configuration;
using BookingManagementService.Infrastructure.Configurations.Redis.interfaces;

namespace BookingManagementService.Infrastructure.Configurations.Redis
{
    public class RedisSessionConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }
    }
    public class RedisSessionConfig : IRedisSessionConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }

        public RedisSessionConfig(IConfiguration configuration)
        {
            var sessionConfig = configuration.GetSection("Infrastructure:Redis:Session").Get<RedisSessionConfigModel>();
            if (sessionConfig != null)
            {
                KeyPrefix = sessionConfig.KeyPrefix;
                ExpirySeconds = sessionConfig.ExpirySeconds;
            }
        }
    }
} 