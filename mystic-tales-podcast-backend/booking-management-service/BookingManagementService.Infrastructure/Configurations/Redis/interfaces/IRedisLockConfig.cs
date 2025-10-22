namespace BookingManagementService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisLockConfig
    {
        string KeyPrefix { get; set; }
        int ExpirySeconds { get; set; }
    }
} 