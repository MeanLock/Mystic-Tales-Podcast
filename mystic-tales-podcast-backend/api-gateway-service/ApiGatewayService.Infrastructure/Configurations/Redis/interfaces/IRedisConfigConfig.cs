namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisConfigConfig
    {
        string ConfigKeyPrefix { get; set; }
        int ConfigCacheTtlMinutes { get; set; }
        bool EnableConfigCaching { get; set; }
        bool EnableConfigWatching { get; set; }
        int ConfigRefreshIntervalSeconds { get; set; }
    }
}
