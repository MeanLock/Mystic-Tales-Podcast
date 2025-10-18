using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Services.SignalRHubServices;

namespace ModerationService.BusinessLogic.Registrations
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
