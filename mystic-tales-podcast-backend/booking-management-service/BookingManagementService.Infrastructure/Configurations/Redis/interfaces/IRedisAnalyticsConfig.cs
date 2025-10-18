namespace BookingManagementService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisAnalyticsConfig
    {
        string KeyPrefix { get; set; }
        int RetentionDays { get; set; }
    }
} 