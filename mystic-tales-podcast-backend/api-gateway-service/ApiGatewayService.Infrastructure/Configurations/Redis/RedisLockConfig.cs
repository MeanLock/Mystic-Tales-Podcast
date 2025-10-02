using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisLockConfigModel
    {
        public string LockKeyPrefix { get; set; } = "lock:";
        public int DefaultLockTimeoutSeconds { get; set; } = 30;
        public int LockRetryDelayMs { get; set; } = 100;
        public int MaxLockRetryAttempts { get; set; } = 10;
        public bool EnableDistributedLock { get; set; } = true;
    }

    public class RedisLockConfig : IRedisLockConfig
    {
        public string LockKeyPrefix { get; set; } = "lock:";
        public int DefaultLockTimeoutSeconds { get; set; } = 30;
        public int LockRetryDelayMs { get; set; } = 100;
        public int MaxLockRetryAttempts { get; set; } = 10;
        public bool EnableDistributedLock { get; set; } = true;

        public RedisLockConfig(IConfiguration configuration)
        {
            var lockConfig = configuration.GetSection("Infrastructure:Redis:Lock")
                .Get<RedisLockConfigModel>();
            if (lockConfig != null)
            {
                LockKeyPrefix = lockConfig.LockKeyPrefix;
                DefaultLockTimeoutSeconds = lockConfig.DefaultLockTimeoutSeconds;
                LockRetryDelayMs = lockConfig.LockRetryDelayMs;
                MaxLockRetryAttempts = lockConfig.MaxLockRetryAttempts;
                EnableDistributedLock = lockConfig.EnableDistributedLock;
            }
        }
    }
}
