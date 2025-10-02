using SagaOrchestratorService.BusinessLogic.Services.SagaServices;
using SagaOrchestratorService.BusinessLogic.Services.SagaServices.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace SagaOrchestratorService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            services.AddScoped<ISagaService, SagaService>();
            return services;
        }
    }
}
