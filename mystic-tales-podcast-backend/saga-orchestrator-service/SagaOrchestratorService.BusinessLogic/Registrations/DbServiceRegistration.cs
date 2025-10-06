using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.BusinessLogic.Services.DbServices.SagaServices;

namespace SagaOrchestratorService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            services.AddScoped<SagaInstanceService>();
            return services;
        }
    }
}
