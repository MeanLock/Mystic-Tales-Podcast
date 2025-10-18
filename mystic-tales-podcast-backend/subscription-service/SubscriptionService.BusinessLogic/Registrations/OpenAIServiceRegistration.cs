using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.BusinessLogic.Services.OpenAIServices;

namespace SubscriptionService.BusinessLogic.Registrations
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
