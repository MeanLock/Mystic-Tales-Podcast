using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Redis
{
    public class RedisLockConfigModel
    {
        public string KeyPrefix { get; set; }
        public int ExpirySeconds { get; set; }
    }
    public class RedisLockConfig : IRedisLockConfig
    {
        public string KeyPrefix { get; set; }
        public int ExpirySeconds { get; set; }

        public RedisLockConfig(IConfiguration configuration)
        {
            var lockConfig = configuration.GetSection("Infrastructure:Redis:Lock").Get<RedisLockConfigModel>();
            KeyPrefix = lockConfig.KeyPrefix;
            ExpirySeconds = lockConfig.ExpirySeconds;
        }
    }
} 