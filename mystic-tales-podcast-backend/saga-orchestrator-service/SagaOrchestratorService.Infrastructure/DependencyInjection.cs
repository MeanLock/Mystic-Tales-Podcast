using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.Infrastructure.Registrations;

namespace SagaOrchestratorService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfiguration();
            services.AddServices(configuration);
            return services;
        }
    }
}
