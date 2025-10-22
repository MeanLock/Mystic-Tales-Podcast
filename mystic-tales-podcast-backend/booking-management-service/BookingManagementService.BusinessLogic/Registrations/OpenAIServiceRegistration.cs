using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.BusinessLogic.Services.OpenAIServices;

namespace BookingManagementService.BusinessLogic.Registrations
{
    public static class OpenAIServiceRegistration
    {
        public static IServiceCollection AddOpenAIServices(this IServiceCollection services)
        {
            services.AddScoped<SurveyOpenAIService>();
            return services;
        }
    }
}
