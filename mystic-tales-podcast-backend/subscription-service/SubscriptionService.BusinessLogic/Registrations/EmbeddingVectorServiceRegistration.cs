using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.BusinessLogic.Services.EmbeddingVectorServices;

namespace SubscriptionService.BusinessLogic.Registrations
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
