using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.BusinessLogic.Services.EmbeddingVectorServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
{
    public static class EmbeddingVectorServiceRegistration
    {
        public static IServiceCollection AddEmbeddingVectorServices(this IServiceCollection services)
        {
            services.AddScoped<SurveyEmbeddingVectorService>();

            return services;
        }
    }
}
