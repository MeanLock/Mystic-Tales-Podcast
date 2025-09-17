namespace Architecture_1.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisMessageQueueConfig
    {
        string KeyPrefix { get; set; }
        int ExpirySeconds { get; set; }
    }
} 