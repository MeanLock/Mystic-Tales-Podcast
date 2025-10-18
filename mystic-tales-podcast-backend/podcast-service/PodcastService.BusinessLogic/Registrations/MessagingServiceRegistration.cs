using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.BusinessLogic.Services.MessagingServices;
using PodcastService.BusinessLogic.MessageHandlers;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;

namespace PodcastService.BusinessLogic.Registrations
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
            
            services.AddScoped<AuthMessagingService>();
            return services;
        }
    }
}
