using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.Common.Registrations;

namespace SagaOrchestratorService.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonLayer(this IServiceCollection services)
        {
            services.AddConfiguration();
            return services;
        }
    }
}
