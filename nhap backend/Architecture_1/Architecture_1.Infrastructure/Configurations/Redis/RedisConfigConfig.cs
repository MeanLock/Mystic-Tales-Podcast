using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Redis
{
    public class RedisConfigConfigModel
    {
        public string KeyPrefix { get; set; }
        public int RefreshIntervalSeconds { get; set; }
    }
    public class RedisConfigConfig : IRedisConfigConfig
    {
        public string KeyPrefix { get; set; }
        public int RefreshIntervalSeconds { get; set; }

        public RedisConfigConfig(IConfiguration configuration)
        {
            var configConfig = configuration.GetSection("Infrastructure:Redis:Config").Get<RedisConfigConfigModel>();
            KeyPrefix = configConfig.KeyPrefix;
            RefreshIntervalSeconds = configConfig.RefreshIntervalSeconds;
        }
    }
} 