namespace SagaOrchestratorService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisJobQueueConfig
    {
        string KeyPrefix { get; set; }
        int MaxRetries { get; set; }
        int RetryDelaySeconds { get; set; }
    }
} 