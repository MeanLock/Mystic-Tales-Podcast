using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.EmbeddingVectorServices;

namespace PodcastService.BusinessLogic.Registrations
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
