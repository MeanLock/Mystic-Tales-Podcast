using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Redis
{
    public class RedisSessionConfigModel
    {
        public string KeyPrefix { get; set; }
        public int ExpirySeconds { get; set; }
    }
    public class RedisSessionConfig : IRedisSessionConfig
    {
        public string KeyPrefix { get; set; }
        public int ExpirySeconds { get; set; }

        public RedisSessionConfig(IConfiguration configuration)
        {
            var sessionConfig = configuration.GetSection("Infrastructure:Redis:Session").Get<RedisSessionConfigModel>();
            KeyPrefix = sessionConfig.KeyPrefix;
            ExpirySeconds = sessionConfig.ExpirySeconds;
        }
    }
} 