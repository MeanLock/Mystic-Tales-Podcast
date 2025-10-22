namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisJobQueueConfig
    {
        string JobQueuePrefix { get; set; }
        int MaxConcurrentJobs { get; set; }
        int JobTimeoutMinutes { get; set; }
        int RetryDelaySeconds { get; set; }
        int MaxRetryAttempts { get; set; }
        bool EnableJobPersistence { get; set; }
    }
}
