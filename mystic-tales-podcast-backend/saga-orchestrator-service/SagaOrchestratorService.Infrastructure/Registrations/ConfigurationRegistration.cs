using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.Infrastructure.Configurations.Kafka.interfaces;
using SagaOrchestratorService.Infrastructure.Configurations.Kafka;


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
            
            return services;
        }
    }
}
