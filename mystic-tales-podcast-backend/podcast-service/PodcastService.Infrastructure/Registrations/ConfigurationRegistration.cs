using Microsoft.Extensions.DependencyInjection;
using PodcastService.Infrastructure.Configurations.AWS;
using PodcastService.Infrastructure.Configurations.AWS.interfaces;
using PodcastService.Infrastructure.Configurations.Google;
using PodcastService.Infrastructure.Configurations.Google.interfaces;
using PodcastService.Infrastructure.Configurations.Redis.interfaces;
using PodcastService.Infrastructure.Configurations.Redis;
using PodcastService.Infrastructure.Configurations.OpenAI.interfaces;
using PodcastService.Infrastructure.Configurations.OpenAI;
using PodcastService.Infrastructure.Configurations.Payos.interfaces;
using PodcastService.Infrastructure.Configurations.Payos;
using PodcastService.Common.Configurations.Consul.interfaces;
using PodcastService.Common.Configurations.Consul;
using PodcastService.Infrastructure.Configurations.Kafka.interfaces;
using PodcastService.Infrastructure.Configurations.Kafka;
using PodcastService.Infrastructure.Configurations.Audio.Hls.interfaces;
using PodcastService.Infrastructure.Configurations.Audio.Hls;
using PodcastService.Infrastructure.Configurations.Audio.Tuning;

namespace PodcastService.Infrastructure.Registrations
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
            services.AddSingleton<IEqualizerConfig, EqualizerConfig>(); 
            
            return services;
        }
    }
}
