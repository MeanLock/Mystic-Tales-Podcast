using SagaOrchestratorService.BusinessLogic.Registrations;
using Microsoft.Extensions.DependencyInjection;

namespace SagaOrchestratorService.BusinessLogic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            //services.AddConfiguration();
            services.AddBackgroundServices();
            services.AddDbServices();    
            services.AddHelpers();
            services.AddMessagingServices(); // 🔧 NEW
            return services;
        }
    }
}
