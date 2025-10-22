using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.BusinessLogic.Services.SignalRHubServices;

namespace SubscriptionService.BusinessLogic.Registrations
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
