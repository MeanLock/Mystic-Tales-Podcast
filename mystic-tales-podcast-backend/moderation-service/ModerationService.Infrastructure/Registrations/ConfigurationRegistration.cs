using Microsoft.Extensions.DependencyInjection;
using ModerationService.Infrastructure.Configurations.AWS;
using ModerationService.Infrastructure.Configurations.AWS.interfaces;
using ModerationService.Infrastructure.Configurations.Google;
using ModerationService.Infrastructure.Configurations.Google.interfaces;
using ModerationService.Infrastructure.Configurations.Redis.interfaces;
using ModerationService.Infrastructure.Configurations.Redis;
using ModerationService.Infrastructure.Configurations.OpenAI.interfaces;
using ModerationService.Infrastructure.Configurations.OpenAI;
using ModerationService.Infrastructure.Configurations.Payos.interfaces;
using ModerationService.Infrastructure.Configurations.Payos;
using ModerationService.Common.Configurations.Consul.interfaces;
using ModerationService.Common.Configurations.Consul;
using ModerationService.Infrastructure.Configurations.Kafka.interfaces;
using ModerationService.Infrastructure.Configurations.Kafka;

namespace ModerationService.Infrastructure.Registrations
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
