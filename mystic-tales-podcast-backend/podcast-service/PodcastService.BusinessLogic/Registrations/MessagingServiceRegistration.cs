using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.BusinessLogic.Services.MessagingServices;
using PodcastService.BusinessLogic.MessageHandlers;

namespace PodcastService.BusinessLogic.Registrations
{
    public static class MessagingServiceRegistration
    {
        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            // Message Handlers
            services.AddScoped<ContentManagementDomainMessageHandler>();
            services.AddScoped<PublicReviewManagementDomainMessageHandler>();
            
            // Messaging Services (trong MessagingServices folder)
            services.AddScoped<IMessagingService, MessagingService>();
            
            // Handler Registry and Background Service
            services.AddSingleton<IHandlerRegistryService, HandlerRegistryService>();
            services.AddHostedService<HandlerRegistrationHostedService>();
            
            return services;
        }
    }
}
