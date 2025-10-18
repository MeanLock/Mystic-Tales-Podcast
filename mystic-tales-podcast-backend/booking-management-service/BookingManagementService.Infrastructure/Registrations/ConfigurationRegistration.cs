using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.Infrastructure.Configurations.AWS;
using BookingManagementService.Infrastructure.Configurations.AWS.interfaces;
using BookingManagementService.Infrastructure.Configurations.Google;
using BookingManagementService.Infrastructure.Configurations.Google.interfaces;
using BookingManagementService.Infrastructure.Configurations.Redis.interfaces;
using BookingManagementService.Infrastructure.Configurations.Redis;
using BookingManagementService.Infrastructure.Configurations.OpenAI.interfaces;
using BookingManagementService.Infrastructure.Configurations.OpenAI;
using BookingManagementService.Infrastructure.Configurations.Payos.interfaces;
using BookingManagementService.Infrastructure.Configurations.Payos;
using BookingManagementService.Common.Configurations.Consul.interfaces;
using BookingManagementService.Common.Configurations.Consul;
using BookingManagementService.Infrastructure.Configurations.Kafka.interfaces;
using BookingManagementService.Infrastructure.Configurations.Kafka;

namespace BookingManagementService.Infrastructure.Registrations
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
