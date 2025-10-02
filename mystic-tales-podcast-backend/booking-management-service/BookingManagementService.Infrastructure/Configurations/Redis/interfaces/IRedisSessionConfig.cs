namespace BookingManagementService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisSessionConfig
    {
        string KeyPrefix { get; set; }
        int ExpirySeconds { get; set; }
    }
} 