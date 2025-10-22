using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Services.OpenAIServices;

namespace TransactionService.BusinessLogic.Registrations
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
