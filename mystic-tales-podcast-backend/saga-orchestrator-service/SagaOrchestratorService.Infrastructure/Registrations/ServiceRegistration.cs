using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SagaOrchestratorService.Infrastructure.Services.Kafka;
using SagaOrchestratorService.Infrastructure.Configurations.Kafka.interfaces;
using SagaOrchestratorService.Infrastructure.Configurations.Kafka;
using Consul;
using StackExchange.Redis;
using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using SagaOrchestratorService.Infrastructure.Services.Consul;
using SagaOrchestratorService.Infrastructure.Services.Redis;
using SagaOrchestratorService.Infrastructure.Configurations.Redis;

namespace SagaOrchestratorService.Infrastructure.Registrations
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add service groups
            services.AddKafkaServices(configuration);
            services.AddConsulServices(configuration);
            services.AddRedisServices(configuration);

            return services;
        }

        #region Kafka Services
        private static IServiceCollection AddKafkaServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Kafka Configuration already handled in ConfigurationRegistration

            // Kafka Services
            services.AddSingleton<KafkaProducerService>();

            // Register KafkaConsumerService as Singleton utility service
            services.AddSingleton<KafkaConsumerService>();


            // Health Check
            services.AddSingleton<KafkaHealthCheckService>();
            services.AddHealthChecks()
                .AddCheck<KafkaHealthCheckService>("kafka", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded, new[] { "kafka", "messaging" });

            return services;
        }
        #endregion

        private static IServiceCollection AddConsulServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Consul hosted service
            services.AddHostedService<ConsulRegistrationHostedService>();

            // Configure Consul client
            var consulServiceHost = configuration.GetSection("Infrastructure:Consul:Service:Host").Value ?? "http://localhost:8500";

            services.AddSingleton<IConsulClient>(provider =>
            {
                return new ConsulClient(config =>
                {
                    config.Address = new Uri(consulServiceHost);
                });
            });

            return services;
        }

                private static IServiceCollection AddRedisServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Redis service
            services.AddScoped<RedisInstanceCacheService>();
            services.AddScoped<RedisSharedCacheService>();

            // Configure Redis connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = configuration.GetSection("Infrastructure:Redis:Default").Get<RedisDefaultConfigModel>();

                if (config != null && !string.IsNullOrEmpty(config.ConnectionString))
                {
                    var options = new ConfigurationOptions
                    {
                        EndPoints = { config.ConnectionString },
                        Password = config.Password,
                        ConnectTimeout = config.ConnectTimeout,
                        SyncTimeout = config.SyncTimeout,
                        AbortOnConnectFail = config.AbortOnConnectFail,
                        ConnectRetry = config.ConnectRetry,
                        Ssl = config.UseSsl,
                        DefaultDatabase = config.DefaultDatabase
                    };

                    return ConnectionMultiplexer.Connect(options);
                }

                // Fallback configuration if config is null
                return ConnectionMultiplexer.Connect("localhost:6379");
            });

            return services;
        }


    }
}
