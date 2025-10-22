using Microsoft.Extensions.Configuration;
using ModerationService.Infrastructure.Configurations.Redis.interfaces;

namespace ModerationService.Infrastructure.Configurations.Redis
{
    public class RedisMessageQueueConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }
    }
    public class RedisMessageQueueConfig : IRedisMessageQueueConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int ExpirySeconds { get; set; }

        public RedisMessageQueueConfig(IConfiguration configuration)
        {
            var messageQueueConfig = configuration.GetSection("Infrastructure:Redis:MessageQueue").Get<RedisMessageQueueConfigModel>();
            if (messageQueueConfig != null)
            {
                KeyPrefix = messageQueueConfig.KeyPrefix;
                ExpirySeconds = messageQueueConfig.ExpirySeconds;
            }
        }
    }
} 