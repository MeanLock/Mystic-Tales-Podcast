using Microsoft.Extensions.Configuration;
using SurveyTalkService.Infrastructure.Configurations.Redis.interfaces;

namespace SurveyTalkService.Infrastructure.Configurations.Redis
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