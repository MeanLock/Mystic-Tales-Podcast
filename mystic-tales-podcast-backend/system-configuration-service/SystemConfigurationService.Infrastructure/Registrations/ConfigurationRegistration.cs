using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.Infrastructure.Configurations.AWS;
using SystemConfigurationService.Infrastructure.Configurations.AWS.interfaces;
using SystemConfigurationService.Infrastructure.Configurations.Google;
using SystemConfigurationService.Infrastructure.Configurations.Google.interfaces;
using SystemConfigurationService.Infrastructure.Configurations.Redis.interfaces;
using SystemConfigurationService.Infrastructure.Configurations.Redis;
using SystemConfigurationService.Infrastructure.Configurations.OpenAI.interfaces;
using SystemConfigurationService.Infrastructure.Configurations.OpenAI;
using SystemConfigurationService.Infrastructure.Configurations.Payos.interfaces;
using SystemConfigurationService.Infrastructure.Configurations.Payos;
using SystemConfigurationService.Common.Configurations.Consul.interfaces;
using SystemConfigurationService.Common.Configurations.Consul;
using SystemConfigurationService.Infrastructure.Configurations.Kafka.interfaces;
using SystemConfigurationService.Infrastructure.Configurations.Kafka;

namespace SystemConfigurationService.Infrastructure.Registrations
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
            services.AddSingleton<IConsulDistributedLockConfig, ConsulDistributedLockConfig>();

            
            return services;
        }
    }
}
