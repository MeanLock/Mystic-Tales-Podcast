using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Consul;
using Yarp.ReverseProxy.Configuration; // Add for IProxyConfigProvider
using ApiGatewayService.Infrastructure.Services.Yarp;
using ApiGatewayService.Infrastructure.Services.Consul;
using ApiGatewayService.Infrastructure.Services.Redis;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Consul.interfaces;
using ApiGatewayService.Infrastructure.Configurations.Yarp;
using YarpReverseProxyConfig = Yarp.ReverseProxy.Configuration;

namespace ApiGatewayService.Infrastructure.Registrations
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis Services
            services.AddRedisServices(configuration);

            // Consul Services
            services.AddConsulServices(configuration);

            // YARP Services
            services.AddYarpServices(configuration);

            // Health Checks
            services.AddInfrastructureHealthChecks(configuration);

            return services;
        }

        private static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis Connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfig = sp.GetRequiredService<IRedisDefaultConfig>();

                var options = new ConfigurationOptions
                {
                    EndPoints = { redisConfig.ConnectionString },
                    Password = redisConfig.Password,
                    ConnectTimeout = redisConfig.ConnectTimeout,
                    SyncTimeout = redisConfig.SyncTimeout,
                    AbortOnConnectFail = redisConfig.AbortOnConnectFail,
                    ConnectRetry = redisConfig.ConnectRetry,
                    Ssl = redisConfig.UseSsl,
                    DefaultDatabase = redisConfig.DefaultDatabase
                };
                return ConnectionMultiplexer.Connect(options);
            });

            // Redis Services
            services.AddSingleton<RedisCacheService>();
            services.AddSingleton<RedisSessionService>();
            services.AddSingleton<RedisRateLimitService>();
            services.AddSingleton<RedisLockService>();

            return services;
        }

        private static IServiceCollection AddConsulServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Consul Client
            services.AddSingleton<IConsulClient>(sp =>
            {
                var consulServiceConfig = sp.GetRequiredService<IConsulServiceConfig>();
                var consulClientConfig = new ConsulClientConfiguration
                {
                    Address = new Uri(consulServiceConfig.Host)
                };

                return new ConsulClient(consulClientConfig);
            });

            // Consul Services
            services.AddHostedService<ConsulRegistrationHostedService>();

            return services;
        }

        private static IServiceCollection AddYarpServices(this IServiceCollection services, IConfiguration configuration)
        {
            var enableDynamicRouting = configuration.GetValue<bool>("Infrastructure:Yarp:Routing:EnableDynamicRouting", true);

            if (enableDynamicRouting)
            {
                
                services.AddSingleton<YarpServiceDiscoveryService>();
                services.AddSingleton<YarpConfigurationService>();
                services.AddSingleton<YarpDynamicProxyConfigProvider>();
                services.AddHostedService<YarpProxyHostedService>();

                services.AddReverseProxy();
                services.AddSingleton<IProxyConfigProvider>(provider => 
                    provider.GetRequiredService<YarpDynamicProxyConfigProvider>());
            }
            else
            {
                // Static routing: Load từ appsettings.json  
                services.AddReverseProxy()
                    .LoadFromConfig(configuration.GetSection("Infrastructure:Yarp:ReverseProxy"));
            }

            return services;
        }

        private static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            // Redis Health Check
            healthChecksBuilder.AddCheck("redis", () =>
            {
                try
                {
                    // Simple Redis check - will be improved with proper DI
                    var connectionString = configuration["Infrastructure:Redis:Default:ConnectionString"];
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return HealthCheckResult.Unhealthy("Redis connection string not configured");
                    }
                    return HealthCheckResult.Healthy("Redis configuration is valid");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("Redis health check failed", ex);
                }
            });

            // Consul Health Check
            healthChecksBuilder.AddCheck("consul", () =>
            {
                try
                {
                    var consulHost = configuration["Infrastructure:Consul:Service:Host"];
                    var consulServiceName = configuration["Infrastructure:Consul:Service:ServiceName"];
                    var consulHealthCheckEnabled = configuration["Infrastructure:Consul:HealthCheck:Enabled"];

                    if (string.IsNullOrEmpty(consulHost) || string.IsNullOrEmpty(consulServiceName))
                    {
                        return HealthCheckResult.Unhealthy("Consul configuration not complete");
                    }
                    return HealthCheckResult.Healthy("Consul configuration is valid");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("Consul health check failed", ex);
                }
            });

            return services;
        }
    }
}
