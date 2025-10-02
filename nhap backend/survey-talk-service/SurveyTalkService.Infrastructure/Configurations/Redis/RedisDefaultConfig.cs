using Microsoft.Extensions.Configuration;
using SurveyTalkService.Infrastructure.Configurations.AWS.interfaces;
using SurveyTalkService.Infrastructure.Configurations.Redis.interfaces;

namespace SurveyTalkService.Infrastructure.Configurations.Redis
{
    public class RedisDefaultConfigModel
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = string.Empty;
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
        public string InstanceName { get; set; } = string.Empty;
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
                    InstanceName = redisDefaultConfig.InstanceName;
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
