using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisAnalyticsConfigModel
    {
        public string AnalyticsKeyPrefix { get; set; } = "analytics:";
        public int DataRetentionDays { get; set; } = 30;
        public bool EnableRealTimeAnalytics { get; set; } = true;
        public int BatchSize { get; set; } = 100;
        public int FlushIntervalSeconds { get; set; } = 10;
    }

    public class RedisAnalyticsConfig : IRedisAnalyticsConfig
    {
        public string AnalyticsKeyPrefix { get; set; } = "analytics:";
        public int DataRetentionDays { get; set; } = 30;
        public bool EnableRealTimeAnalytics { get; set; } = true;
        public int BatchSize { get; set; } = 100;
        public int FlushIntervalSeconds { get; set; } = 10;

        public RedisAnalyticsConfig(IConfiguration configuration)
        {
            var analyticsConfig = configuration.GetSection("Infrastructure:Redis:Analytics")
                .Get<RedisAnalyticsConfigModel>();
            if (analyticsConfig != null)
            {
                AnalyticsKeyPrefix = analyticsConfig.AnalyticsKeyPrefix;
                DataRetentionDays = analyticsConfig.DataRetentionDays;
                EnableRealTimeAnalytics = analyticsConfig.EnableRealTimeAnalytics;
                BatchSize = analyticsConfig.BatchSize;
                FlushIntervalSeconds = analyticsConfig.FlushIntervalSeconds;
            }
        }
    }
}
