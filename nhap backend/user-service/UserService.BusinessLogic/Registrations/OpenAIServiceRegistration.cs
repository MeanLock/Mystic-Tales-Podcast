using Microsoft.Extensions.DependencyInjection;
using UserService.BusinessLogic.Services.OpenAIServices;

namespace UserService.BusinessLogic.Registrations
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
