using StackExchange.Redis;
// using SurveyTalkService.Infrastructure.AppConfigurations.Redis;

namespace ApiGatewayService.Entry.Configurations.Builder
{
    public static class RedisConfig
    {

        public static void AddBuilderRedisConfig(this WebApplicationBuilder builder)
        {
            // Redis configuration is now handled by Infrastructure layer
            // See ApiGatewayService.Infrastructure.Registrations.ServiceRegistration
            
            // builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            // {
            //     var config = builder.Configuration.GetSection("Infrastructure:Redis:Default").Get<RedisDefaultConfigModel>();
            //     
            //     var options = new ConfigurationOptions
            //     {
            //         EndPoints = { config.ConnectionString },
            //         Password = config.Password,
            //         ConnectTimeout = config.ConnectTimeout,
            //         SyncTimeout = config.SyncTimeout,
            //         AbortOnConnectFail = config.AbortOnConnectFail,
            //         ConnectRetry = config.ConnectRetry,
            //         Ssl = config.UseSsl,
            //         DefaultDatabase = config.DefaultDatabase
            //     };
            //     return ConnectionMultiplexer.Connect(options);
            // });
        }


    }
}
