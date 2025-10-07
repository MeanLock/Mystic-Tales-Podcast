using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.BusinessLogic.Services.MessagingServices.interfaces;
using SubscriptionService.BusinessLogic.Services.MessagingServices;
using SubscriptionService.BusinessLogic.MessageHandlers;

namespace SubscriptionService.BusinessLogic.Registrations
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
