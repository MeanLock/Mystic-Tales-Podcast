using Microsoft.Extensions.Configuration;
using SubscriptionService.Infrastructure.Configurations.AWS.interfaces;
using SubscriptionService.Infrastructure.Configurations.Redis.interfaces;

namespace SubscriptionService.Infrastructure.Configurations.Redis
{
    public class RedisDefaultConfigModel
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceKeyName { get; set; } = string.Empty;
        public string SharedKeyName { get; set; } = string.Empty;
        public int DefaultDatabase { get; set; }
        public int ConnectTimeout { get; set; }
        public int SyncTimeout { get; set; }
        public bool AbortOnConnectFail { get; set; }
        public int ConnectRetry { get; set; }
        public bool UseSsl { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class RedisDefaultConfig : IRedisDefaultConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceKeyName { get; set; } = string.Empty;
        public string SharedKeyName { get; set; } = string.Empty;
        public int DefaultDatabase { get; set; }
        public int ConnectTimeout { get; set; }
        public int SyncTimeout { get; set; }
        public bool AbortOnConnectFail { get; set; }
        public int ConnectRetry { get; set; }
        public bool UseSsl { get; set; }
        public string Password { get; set; } = string.Empty;


        public RedisDefaultConfig(IConfiguration configuration)
        {
            try
            {
                var redisDefaultConfig = configuration.GetSection("Infrastructure:Redis:Default").Get<RedisDefaultConfigModel>();
                if (redisDefaultConfig != null)
                {
                    ConnectionString = redisDefaultConfig.ConnectionString;
                    InstanceKeyName = redisDefaultConfig.InstanceKeyName;
                    SharedKeyName = redisDefaultConfig.SharedKeyName;
                    DefaultDatabase = redisDefaultConfig.DefaultDatabase;
                    ConnectTimeout = redisDefaultConfig.ConnectTimeout;
                    SyncTimeout = redisDefaultConfig.SyncTimeout;
                    AbortOnConnectFail = redisDefaultConfig.AbortOnConnectFail;
                    ConnectRetry = redisDefaultConfig.ConnectRetry;
                    UseSsl = redisDefaultConfig.UseSsl;
                    Password = redisDefaultConfig.Password;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RedisDefaultConfig: {ex.StackTrace}");
            }



        }
    }
}
