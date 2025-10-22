using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisMessageQueueConfigModel
    {
        public string QueueNamePrefix { get; set; } = "queue:";
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public bool EnableDeadLetterQueue { get; set; } = true;
        public int MessageTimeoutMinutes { get; set; } = 30;
    }

    public class RedisMessageQueueConfig : IRedisMessageQueueConfig
    {
        public string QueueNamePrefix { get; set; } = "queue:";
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public bool EnableDeadLetterQueue { get; set; } = true;
        public int MessageTimeoutMinutes { get; set; } = 30;

        public RedisMessageQueueConfig(IConfiguration configuration)
        {
            var messageQueueConfig = configuration.GetSection("Infrastructure:Redis:MessageQueue")
                .Get<RedisMessageQueueConfigModel>();
            if (messageQueueConfig != null)
            {
                QueueNamePrefix = messageQueueConfig.QueueNamePrefix;
                MaxRetryAttempts = messageQueueConfig.MaxRetryAttempts;
                RetryDelaySeconds = messageQueueConfig.RetryDelaySeconds;
                EnableDeadLetterQueue = messageQueueConfig.EnableDeadLetterQueue;
                MessageTimeoutMinutes = messageQueueConfig.MessageTimeoutMinutes;
            }
        }
    }
}
