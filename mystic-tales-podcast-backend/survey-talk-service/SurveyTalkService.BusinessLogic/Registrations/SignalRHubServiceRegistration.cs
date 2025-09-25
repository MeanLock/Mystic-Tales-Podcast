using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.BusinessLogic.Services.SignalRHubServices;

namespace SurveyTalkService.BusinessLogic.Registrations
{
    public static class SignalRHubServiceRegistration
    {
        public static IServiceCollection AddSignalRHubServices(this IServiceCollection services)
        {
            // services.AddSingleton<OpenAIWhisperWebSocketClient>();
            services.AddTransient<OpenAIWhisperService>();
            return services;
        }
    }
}
