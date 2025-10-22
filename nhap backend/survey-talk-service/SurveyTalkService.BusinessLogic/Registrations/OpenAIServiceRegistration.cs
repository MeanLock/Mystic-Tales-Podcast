using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.BusinessLogic.Services.OpenAIServices;

namespace SurveyTalkService.BusinessLogic.Registrations
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
