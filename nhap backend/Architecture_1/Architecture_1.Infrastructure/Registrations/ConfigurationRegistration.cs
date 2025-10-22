using Microsoft.Extensions.DependencyInjection;
using Architecture_1.Infrastructure.Configurations.AWS;
using Architecture_1.Infrastructure.Configurations.AWS.interfaces;
using Architecture_1.Infrastructure.Configurations.Google;
using Architecture_1.Infrastructure.Configurations.Google.interfaces;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;
using Architecture_1.Infrastructure.Configurations.Redis;
using Architecture_1.Infrastructure.Configurations.OpenAI.interfaces;
using Architecture_1.Infrastructure.Configurations.OpenAI;
using Architecture_1.Infrastructure.Configurations.Payos.interfaces;
using Architecture_1.Infrastructure.Configurations.Payos;
using Architecture_1.Infrastructure.Configurations.Kafka.interfaces;
using Architecture_1.Infrastructure.Configurations.Kafka;
using Architecture_1.Common.Configurations.Consul.interfaces;
using Architecture_1.Common.Configurations.Consul;

namespace Architecture_1.Infrastructure.Registrations
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
