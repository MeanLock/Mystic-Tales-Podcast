using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisSessionConfigModel
    {
        public string SessionKeyPrefix { get; set; } = "session:";
        public int SessionTimeoutMinutes { get; set; } = 20;
        public bool EnableSlidingExpiration { get; set; } = true;
        public string CookieName { get; set; } = ".AspNetCore.Session";
        public bool HttpOnly { get; set; } = true;
        public bool SecurePolicy { get; set; } = false;
    }

    public class RedisSessionConfig : IRedisSessionConfig
    {
        public string SessionKeyPrefix { get; set; } = "session:";
        public int SessionTimeoutMinutes { get; set; } = 20;
        public bool EnableSlidingExpiration { get; set; } = true;
        public string CookieName { get; set; } = ".AspNetCore.Session";
        public bool HttpOnly { get; set; } = true;
        public bool SecurePolicy { get; set; } = false;

        public RedisSessionConfig(IConfiguration configuration)
        {
            var sessionConfig = configuration.GetSection("Infrastructure:Redis:Session")
                .Get<RedisSessionConfigModel>();
            if (sessionConfig != null)
            {
                SessionKeyPrefix = sessionConfig.SessionKeyPrefix;
                SessionTimeoutMinutes = sessionConfig.SessionTimeoutMinutes;
                EnableSlidingExpiration = sessionConfig.EnableSlidingExpiration;
                CookieName = sessionConfig.CookieName;
                HttpOnly = sessionConfig.HttpOnly;
                SecurePolicy = sessionConfig.SecurePolicy;
            }
        }
    }
}
