using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Services.EmbeddingVectorServices;

namespace ModerationService.BusinessLogic.Registrations
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
