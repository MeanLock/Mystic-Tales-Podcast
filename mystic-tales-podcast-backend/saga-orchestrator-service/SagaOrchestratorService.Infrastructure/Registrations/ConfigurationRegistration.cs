using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.Infrastructure.Configurations.Kafka.interfaces;
using SagaOrchestratorService.Infrastructure.Configurations.Kafka;
using SagaOrchestratorService.Common.Configurations.Consul.interfaces;
using SagaOrchestratorService.Common.Configurations.Consul;


namespace SagaOrchestratorService.Infrastructure.Registrations
{
    public static class ConfigurationRegistration
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
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
