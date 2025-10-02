namespace TransactionService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisConfigConfig
    {
        string KeyPrefix { get; set; }
        int RefreshIntervalSeconds { get; set; }
    }
} 