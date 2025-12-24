using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisDefaultConfigModel
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceKeyName { get; set; } = string.Empty;
        public string SharedKeyName { get; set; } = string.Empty;
        public int DefaultDatabase { get; set; } = 0;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectRetry { get; set; } = 3;
        public bool UseSsl { get; set; } = false;
        public string Password { get; set; } = string.Empty;
    }

    public class RedisDefaultConfig : IRedisDefaultConfig
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceKeyName { get; set; } = string.Empty;
        public string SharedKeyName { get; set; } = string.Empty;
        public int DefaultDatabase { get; set; } = 0;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public bool AbortOnConnectFail { get; set; } = false;
        public int ConnectRetry { get; set; } = 3;
        public bool UseSsl { get; set; } = false;
        public string Password { get; set; } = string.Empty;

        public RedisDefaultConfig(IConfiguration configuration)
        {
            var redisConfig = configuration.GetSection("Infrastructure:Redis:Default")
                .Get<RedisDefaultConfigModel>();
            if (redisConfig != null)
            {
                ConnectionString = redisConfig.ConnectionString;
                InstanceKeyName = redisConfig.InstanceKeyName;
                SharedKeyName = redisConfig.SharedKeyName;
                DefaultDatabase = redisConfig.DefaultDatabase;
                ConnectTimeout = redisConfig.ConnectTimeout;
                SyncTimeout = redisConfig.SyncTimeout;
                AbortOnConnectFail = redisConfig.AbortOnConnectFail;
                ConnectRetry = redisConfig.ConnectRetry;
                UseSsl = redisConfig.UseSsl;
                Password = redisConfig.Password;
            }
        }
    }
}
