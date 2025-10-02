using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.AWS.interfaces;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Redis
{
    public class RedisDefaultConfigModel
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int DefaultDatabase { get; set; }
        public int ConnectTimeout { get; set; }
        public int SyncTimeout { get; set; }
        public bool AbortOnConnectFail { get; set; }
        public int ConnectRetry { get; set; }
        public bool UseSsl { get; set; }
        public string Password { get; set; }
    }

    public class RedisDefaultConfig : IRedisDefaultConfig
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int DefaultDatabase { get; set; }
        public int ConnectTimeout { get; set; }
        public int SyncTimeout { get; set; }
        public bool AbortOnConnectFail { get; set; }
        public int ConnectRetry { get; set; }
        public bool UseSsl { get; set; }
        public string Password { get; set; }


        public RedisDefaultConfig(IConfiguration configuration)
        {
            try
            {
                var redisDefaultConfig = configuration.GetSection("Infrastructure:Redis:Default").Get<RedisDefaultConfigModel>();
                ConnectionString = redisDefaultConfig.ConnectionString;
                InstanceName = redisDefaultConfig.InstanceName;
                DefaultDatabase = redisDefaultConfig.DefaultDatabase;
                ConnectTimeout = redisDefaultConfig.ConnectTimeout;
                SyncTimeout = redisDefaultConfig.SyncTimeout;
                AbortOnConnectFail = redisDefaultConfig.AbortOnConnectFail;
                ConnectRetry = redisDefaultConfig.ConnectRetry;
                UseSsl = redisDefaultConfig.UseSsl;
                Password = redisDefaultConfig.Password;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RedisDefaultConfig: {ex.StackTrace}");
            }



        }
    }
}
