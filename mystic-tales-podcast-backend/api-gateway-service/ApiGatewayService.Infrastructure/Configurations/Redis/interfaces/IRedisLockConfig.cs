namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisLockConfig
    {
        string LockKeyPrefix { get; set; }
        int DefaultLockTimeoutSeconds { get; set; }
        int LockRetryDelayMs { get; set; }
        int MaxLockRetryAttempts { get; set; }
        bool EnableDistributedLock { get; set; }
    }
}
