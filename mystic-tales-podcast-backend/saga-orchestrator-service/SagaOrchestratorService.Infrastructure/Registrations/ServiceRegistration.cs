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

namespace SagaOrchestratorService.Infrastructure.Registrations
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add service groups
            services.AddKafkaServices(configuration);
            services.AddConsulServices(configuration);

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

    }
}
