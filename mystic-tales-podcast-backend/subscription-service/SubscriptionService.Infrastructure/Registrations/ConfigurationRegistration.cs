using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.Infrastructure.Configurations.AWS;
using SubscriptionService.Infrastructure.Configurations.AWS.interfaces;
using SubscriptionService.Infrastructure.Configurations.Google;
using SubscriptionService.Infrastructure.Configurations.Google.interfaces;
using SubscriptionService.Infrastructure.Configurations.Redis.interfaces;
using SubscriptionService.Infrastructure.Configurations.Redis;
using SubscriptionService.Infrastructure.Configurations.OpenAI.interfaces;
using SubscriptionService.Infrastructure.Configurations.OpenAI;
using SubscriptionService.Infrastructure.Configurations.Payos.interfaces;
using SubscriptionService.Infrastructure.Configurations.Payos;
using SubscriptionService.Common.Configurations.Consul.interfaces;
using SubscriptionService.Common.Configurations.Consul;
using SubscriptionService.Infrastructure.Configurations.Kafka.interfaces;
using SubscriptionService.Infrastructure.Configurations.Kafka;

namespace SubscriptionService.Infrastructure.Registrations
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
