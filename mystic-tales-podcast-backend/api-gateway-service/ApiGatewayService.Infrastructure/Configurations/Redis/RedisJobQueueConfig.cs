using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisJobQueueConfigModel
    {
        public string JobQueuePrefix { get; set; } = "job:";
        public int MaxConcurrentJobs { get; set; } = 10;
        public int JobTimeoutMinutes { get; set; } = 30;
        public int RetryDelaySeconds { get; set; } = 5;
        public int MaxRetryAttempts { get; set; } = 3;
        public bool EnableJobPersistence { get; set; } = true;
    }

    public class RedisJobQueueConfig : IRedisJobQueueConfig
    {
        public string JobQueuePrefix { get; set; } = "job:";
        public int MaxConcurrentJobs { get; set; } = 10;
        public int JobTimeoutMinutes { get; set; } = 30;
        public int RetryDelaySeconds { get; set; } = 5;
        public int MaxRetryAttempts { get; set; } = 3;
        public bool EnableJobPersistence { get; set; } = true;

        public RedisJobQueueConfig(IConfiguration configuration)
        {
            var jobQueueConfig = configuration.GetSection("Infrastructure:Redis:JobQueue")
                .Get<RedisJobQueueConfigModel>();
            if (jobQueueConfig != null)
            {
                JobQueuePrefix = jobQueueConfig.JobQueuePrefix;
                MaxConcurrentJobs = jobQueueConfig.MaxConcurrentJobs;
                JobTimeoutMinutes = jobQueueConfig.JobTimeoutMinutes;
                RetryDelaySeconds = jobQueueConfig.RetryDelaySeconds;
                MaxRetryAttempts = jobQueueConfig.MaxRetryAttempts;
                EnableJobPersistence = jobQueueConfig.EnableJobPersistence;
            }
        }
    }
}
