using Microsoft.Extensions.Configuration;
using ModerationService.Infrastructure.Configurations.Redis.interfaces;

namespace ModerationService.Infrastructure.Configurations.Redis
{
    public class RedisAnalyticsConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int RetentionDays { get; set; }
    }
    public class RedisAnalyticsConfig : IRedisAnalyticsConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int RetentionDays { get; set; }

        public RedisAnalyticsConfig(IConfiguration configuration)
        {
            var analyticsConfig = configuration.GetSection("Infrastructure:Redis:Analytics").Get<RedisAnalyticsConfigModel>();
            if (analyticsConfig != null)
            {
                KeyPrefix = analyticsConfig.KeyPrefix;
                RetentionDays = analyticsConfig.RetentionDays;
            }
        }
    }
} 