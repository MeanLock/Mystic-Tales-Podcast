using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Services.OpenAIServices;

namespace ModerationService.BusinessLogic.Registrations
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
