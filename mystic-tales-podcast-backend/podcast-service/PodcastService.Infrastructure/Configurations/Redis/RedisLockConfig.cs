using Microsoft.Extensions.Configuration;
using PodcastService.Infrastructure.Configurations.Redis.interfaces;

namespace PodcastService.Infrastructure.Configurations.Redis
{
    public class RedisLockConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }
    }
    public class RedisLockConfig : IRedisLockConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }

        public RedisLockConfig(IConfiguration configuration)
        {
            var lockConfig = configuration.GetSection("Infrastructure:Redis:Lock").Get<RedisLockConfigModel>();
            if (lockConfig != null)
            {
                KeyPrefix = lockConfig.KeyPrefix;
                ExpirySeconds = lockConfig.ExpirySeconds;
            }
        }
    }
} 