using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.BusinessLogic.Services.MessagingServices.interfaces;
using SagaOrchestratorService.BusinessLogic.Services.MessagingServices;
using SagaOrchestratorService.BusinessLogic.MessageHandlers;

namespace SagaOrchestratorService.BusinessLogic.Registrations
{
    public static class MessagingServiceRegistration
    {
        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            // Saga handlers + routing store
            services.AddScoped<FlowMessageHandler>();
            services.AddScoped<FlowStepEmitMessageHandler>();

            services.AddScoped<IMessagingService, MessagingService>();
            services.AddSingleton<IHandlerRegistryService, HandlerRegistryService>();
            services.AddHostedService<HandlerRegistrationHostedService>();

            return services;
        }
    }
}
