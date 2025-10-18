using Microsoft.Extensions.Configuration;
using SurveyTalkService.Infrastructure.Configurations.Redis.interfaces;

namespace SurveyTalkService.Infrastructure.Configurations.Redis
{
    public class RedisJobQueueConfigModel
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int MaxRetries { get; set; }
        public int RetryDelaySeconds { get; set; }
    }
    public class RedisJobQueueConfig : IRedisJobQueueConfig
    {
        public string KeyPrefix { get; set; } = string.Empty;
        public int MaxRetries { get; set; }
        public int RetryDelaySeconds { get; set; }

        public RedisJobQueueConfig(IConfiguration configuration)
        {
            var jobQueueConfig = configuration.GetSection("Infrastructure:Redis:JobQueue").Get<RedisJobQueueConfigModel>();
            if (jobQueueConfig != null)
            {
                KeyPrefix = jobQueueConfig.KeyPrefix;
                MaxRetries = jobQueueConfig.MaxRetries;
                RetryDelaySeconds = jobQueueConfig.RetryDelaySeconds;
            }
        }
    }
} 