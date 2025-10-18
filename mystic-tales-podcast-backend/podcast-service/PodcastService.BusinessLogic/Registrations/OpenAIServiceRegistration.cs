using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.OpenAIServices;

namespace PodcastService.BusinessLogic.Registrations
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
