using Microsoft.Extensions.DependencyInjection;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Redis;
using ApiGatewayService.Infrastructure.Configurations.Consul.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Consul;
using ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Yarp;

namespace ApiGatewayService.Infrastructure.Registrations
{
    public static class ConfigurationRegistration
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
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

            // Consul - Infrastructure configs
            services.AddSingleton<IConsulServiceConfig, ConsulServiceConfig>();
            services.AddSingleton<IConsulHealthCheckConfig, ConsulHealthCheckConfig>();

            // YARP 
            services.AddSingleton<IYarpReverseProxyConfig, YarpReverseProxyConfig>();
            services.AddSingleton<IYarpRoutingConfig, YarpRoutingConfig>();

            return services;
        }
    }
}
