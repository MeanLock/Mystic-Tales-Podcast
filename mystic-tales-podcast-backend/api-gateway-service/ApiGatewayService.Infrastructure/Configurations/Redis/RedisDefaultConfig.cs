using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisDefaultConfigModel
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "ApiGatewayService";
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
        public string InstanceName { get; set; } = "ApiGatewayService";
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
                InstanceName = redisConfig.InstanceName;
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
