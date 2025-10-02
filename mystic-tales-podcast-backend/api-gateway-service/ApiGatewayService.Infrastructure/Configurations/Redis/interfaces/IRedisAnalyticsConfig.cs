namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisAnalyticsConfig
    {
        string AnalyticsKeyPrefix { get; set; }
        int DataRetentionDays { get; set; }
        bool EnableRealTimeAnalytics { get; set; }
        int BatchSize { get; set; }
        int FlushIntervalSeconds { get; set; }
    }
}
