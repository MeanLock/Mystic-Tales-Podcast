using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.BusinessLogic.Services.EmbeddingVectorServices;

namespace BookingManagementService.BusinessLogic.Registrations
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
