using Microsoft.Extensions.DependencyInjection;
using TransactionService.Infrastructure.Configurations.AWS;
using TransactionService.Infrastructure.Configurations.AWS.interfaces;
using TransactionService.Infrastructure.Configurations.Google;
using TransactionService.Infrastructure.Configurations.Google.interfaces;
using TransactionService.Infrastructure.Configurations.Redis.interfaces;
using TransactionService.Infrastructure.Configurations.Redis;
using TransactionService.Infrastructure.Configurations.OpenAI.interfaces;
using TransactionService.Infrastructure.Configurations.OpenAI;
using TransactionService.Infrastructure.Configurations.Payos.interfaces;
using TransactionService.Infrastructure.Configurations.Payos;
using TransactionService.Common.Configurations.Consul.interfaces;
using TransactionService.Common.Configurations.Consul;
using TransactionService.Infrastructure.Configurations.Kafka.interfaces;
using TransactionService.Infrastructure.Configurations.Kafka;

namespace TransactionService.Infrastructure.Registrations
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
