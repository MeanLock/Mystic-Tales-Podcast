using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisConfigConfigModel
    {
        public string ConfigKeyPrefix { get; set; } = "config:";
        public int ConfigCacheTtlMinutes { get; set; } = 60;
        public bool EnableConfigCaching { get; set; } = true;
        public bool EnableConfigWatching { get; set; } = true;
        public int ConfigRefreshIntervalSeconds { get; set; } = 30;
    }

    public class RedisConfigConfig : IRedisConfigConfig
    {
        public string ConfigKeyPrefix { get; set; } = "config:";
        public int ConfigCacheTtlMinutes { get; set; } = 60;
        public bool EnableConfigCaching { get; set; } = true;
        public bool EnableConfigWatching { get; set; } = true;
        public int ConfigRefreshIntervalSeconds { get; set; } = 30;

        public RedisConfigConfig(IConfiguration configuration)
        {
            var configConfig = configuration.GetSection("Infrastructure:Redis:Config")
                .Get<RedisConfigConfigModel>();
            if (configConfig != null)
            {
                ConfigKeyPrefix = configConfig.ConfigKeyPrefix;
                ConfigCacheTtlMinutes = configConfig.ConfigCacheTtlMinutes;
                EnableConfigCaching = configConfig.EnableConfigCaching;
                EnableConfigWatching = configConfig.EnableConfigWatching;
                ConfigRefreshIntervalSeconds = configConfig.ConfigRefreshIntervalSeconds;
            }
        }
    }
}
