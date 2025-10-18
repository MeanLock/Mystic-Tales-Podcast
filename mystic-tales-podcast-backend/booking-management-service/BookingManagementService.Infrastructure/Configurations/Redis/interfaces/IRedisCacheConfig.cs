namespace BookingManagementService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisCacheConfig
    {
        string KeyPrefix { get; set; }
        int ExpirySeconds { get; set; }
        int SlidingExpirationSeconds { get; set; }
    }
}
