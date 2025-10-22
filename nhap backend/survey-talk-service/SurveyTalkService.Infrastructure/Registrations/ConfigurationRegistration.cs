using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.Infrastructure.Configurations.AWS;
using SurveyTalkService.Infrastructure.Configurations.AWS.interfaces;
using SurveyTalkService.Infrastructure.Configurations.Google;
using SurveyTalkService.Infrastructure.Configurations.Google.interfaces;
using SurveyTalkService.Infrastructure.Configurations.Redis.interfaces;
using SurveyTalkService.Infrastructure.Configurations.Redis;
using SurveyTalkService.Infrastructure.Configurations.OpenAI.interfaces;
using SurveyTalkService.Infrastructure.Configurations.OpenAI;
using SurveyTalkService.Infrastructure.Configurations.Payos.interfaces;
using SurveyTalkService.Infrastructure.Configurations.Payos;
using SurveyTalkService.Common.Configurations.Consul.interfaces;
using SurveyTalkService.Common.Configurations.Consul;
using SurveyTalkService.Infrastructure.Configurations.Kafka.interfaces;
using SurveyTalkService.Infrastructure.Configurations.Kafka;

namespace SurveyTalkService.Infrastructure.Registrations
{
    public static class ConfigurationRegistration
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            
            // AWS
            services.AddSingleton<IAWSS3Config, AWSS3Config>();

            // Google
            services.AddSingleton<IGoogleOAuth2Config, GoogleOAuth2Config>();
            services.AddSingleton<IGoogleMailConfig, GoogleMailConfig>();


            // Redis
            services.AddSingleton<IRedisDefaultConfig, RedisDefaultConfig>();
            services.AddSingleton<IRedisCacheConfig, RedisCacheConfig>();
            services.AddSingleton<IRedisSessionConfig, RedisSessionConfig>();
            services.AddSingleton<IRedisRateLimitConfig, RedisRateLimitConfig>();
            services.AddSingleton<IRedisMessageQueueConfig, RedisMessageQueueConfig>();
            services.AddSingleton<IRedisLockConfig, RedisLockConfig>();
            services.AddSingleton<IRedisAnalyticsConfig, RedisAnalyticsConfig>();
            services.AddSingleton<IRedisConfigConfig, RedisConfigConfig>();
            services.AddSingleton<IRedisJobQueueConfig, RedisJobQueueConfig>();

            // OpenAI
            services.AddSingleton<IOpenAIConfig, OpenAIConfig>();

            // Payos
            services.AddSingleton<IPayosConfig, PayosConfig>();

            // Kafka
            services.AddSingleton<IKafkaClusterConfig, KafkaClusterConfig>();
            services.AddSingleton<IKafkaProducerConfig, KafkaProducerConfig>();
            services.AddSingleton<IKafkaConsumerConfig, KafkaConsumerConfig>();

            // Consul
            services.AddSingleton<IConsulServiceConfig, ConsulServiceConfig>();
            services.AddSingleton<IConsulHealthCheckConfig, ConsulHealthCheckConfig>();

            
            return services;
        }
    }
}
