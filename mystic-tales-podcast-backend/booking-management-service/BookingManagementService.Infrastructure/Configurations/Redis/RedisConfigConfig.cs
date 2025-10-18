using Microsoft.Extensions.Configuration;
using BookingManagementService.Infrastructure.Configurations.Redis.interfaces;

namespace BookingManagementService.Infrastructure.Configurations.Redis
{
    public class RedisConfigConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int RefreshIntervalSeconds { get; set; }
    }
    public class RedisConfigConfig : IRedisConfigConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int RefreshIntervalSeconds { get; set; }

        public RedisConfigConfig(IConfiguration configuration)
        {
            var configConfig = configuration.GetSection("Infrastructure:Redis:Config").Get<RedisConfigConfigModel>();
            if (configConfig != null)
            {
                KeyPrefix = configConfig.KeyPrefix;
                RefreshIntervalSeconds = configConfig.RefreshIntervalSeconds;
            }
        }
    }
} 