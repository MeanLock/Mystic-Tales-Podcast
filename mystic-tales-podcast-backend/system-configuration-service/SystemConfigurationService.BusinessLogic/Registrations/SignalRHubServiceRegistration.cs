using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.BusinessLogic.Services.SignalRHubServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
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
