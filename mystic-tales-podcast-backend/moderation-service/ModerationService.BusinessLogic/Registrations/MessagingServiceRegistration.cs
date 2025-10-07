using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.BusinessLogic.Services.MessagingServices;
using ModerationService.BusinessLogic.MessageHandlers;

namespace ModerationService.BusinessLogic.Registrations
{
    public static class MessagingServiceRegistration
    {
        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            // Message Handlers
            services.AddScoped<AuthMessageHandler>();
            
            // Messaging Services (trong MessagingServices folder)
            services.AddScoped<IMessagingService, MessagingService>();
            
            // Handler Registry and Background Service
            services.AddSingleton<IHandlerRegistryService, HandlerRegistryService>();
            services.AddHostedService<HandlerRegistrationHostedService>();
            
            return services;
        }
    }
}
