namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisMessageQueueConfig
    {
        string QueueNamePrefix { get; set; }
        int MaxRetryAttempts { get; set; }
        int RetryDelaySeconds { get; set; }
        bool EnableDeadLetterQueue { get; set; }
        int MessageTimeoutMinutes { get; set; }
    }
}
