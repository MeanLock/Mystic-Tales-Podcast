using Microsoft.Extensions.DependencyInjection;
using UserService.Infrastructure.Configurations.AWS;
using UserService.Infrastructure.Configurations.AWS.interfaces;
using UserService.Infrastructure.Configurations.Google;
using UserService.Infrastructure.Configurations.Google.interfaces;
using UserService.Infrastructure.Configurations.Redis.interfaces;
using UserService.Infrastructure.Configurations.Redis;
using UserService.Infrastructure.Configurations.OpenAI.interfaces;
using UserService.Infrastructure.Configurations.OpenAI;
using UserService.Infrastructure.Configurations.Payos.interfaces;
using UserService.Infrastructure.Configurations.Payos;
using UserService.Infrastructure.Configurations.Consul.interfaces;
using UserService.Infrastructure.Configurations.Consul;
using UserService.Infrastructure.Configurations.Kafka.interfaces;
using UserService.Infrastructure.Configurations.Kafka;
using UserService.Infrastructure.Configurations.Audio.Hls.interfaces;
using UserService.Infrastructure.Configurations.Audio.Hls;
using UserService.Infrastructure.Configurations.Audio.Tuning;
using UserService.Infrastructure.Configurations.Fonts.interfaces;

namespace UserService.Infrastructure.Registrations
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

            // Audio
            services.AddSingleton<IHlsConfig, HlsConfig>();
            services.AddSingleton<IMoodConfig, MoodConfig>();
            services.AddSingleton<IEqualizerConfig, EqualizerConfig>(); // Add this missing registration

            // Fonts
            services.AddSingleton<IFontProvider, FontProvider>();

            return services;
        }
    }
}
