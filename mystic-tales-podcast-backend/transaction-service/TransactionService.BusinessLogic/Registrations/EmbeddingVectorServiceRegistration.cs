using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Services.EmbeddingVectorServices;

namespace TransactionService.BusinessLogic.Registrations
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
