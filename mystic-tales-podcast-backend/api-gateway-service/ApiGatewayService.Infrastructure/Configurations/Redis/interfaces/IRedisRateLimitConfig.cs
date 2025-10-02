namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisRateLimitConfig
    {
        int DefaultLimit { get; set; }
        int WindowMinutes { get; set; }
        bool EnableRateLimit { get; set; }
        string RateLimitKeyPrefix { get; set; }
        int BurstLimit { get; set; }
    }
}
