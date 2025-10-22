using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.BusinessLogic.Helpers;

namespace SagaOrchestratorService.BusinessLogic.Registrations
{
    public static class HelperRegistration
    {
        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            services.AddSingleton<BcryptHelpers>();
            services.AddSingleton<JwtHelpers>();
            services.AddSingleton<FileHelpers>();
            services.AddSingleton<DateHelpers>();
            return services;
        }
    }
}
