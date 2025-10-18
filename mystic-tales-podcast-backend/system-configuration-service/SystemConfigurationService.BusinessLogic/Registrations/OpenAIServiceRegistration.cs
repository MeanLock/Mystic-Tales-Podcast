using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.BusinessLogic.Services.OpenAIServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
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
