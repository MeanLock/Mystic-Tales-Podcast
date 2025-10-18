namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisSessionConfig
    {
        string SessionKeyPrefix { get; set; }
        int SessionTimeoutMinutes { get; set; }
        bool EnableSlidingExpiration { get; set; }
        string CookieName { get; set; }
        bool HttpOnly { get; set; }
        bool SecurePolicy { get; set; }
    }
}
