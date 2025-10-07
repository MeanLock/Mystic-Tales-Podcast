using Microsoft.Extensions.DependencyInjection;
using UserService.BusinessLogic.Services.SignalRHubServices;

namespace UserService.BusinessLogic.Registrations
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
