using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.BusinessLogic.Services.EmbeddingVectorServices;

namespace SurveyTalkService.BusinessLogic.Registrations
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
