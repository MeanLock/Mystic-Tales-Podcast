using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.BusinessLogic.Services.MessagingServices.interfaces;
using BookingManagementService.BusinessLogic.Services.MessagingServices;
using BookingManagementService.BusinessLogic.MessageHandlers;

namespace BookingManagementService.BusinessLogic.Registrations
{
    public static class MessagingServiceRegistration
    {
        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            // Message Handlers
            services.AddScoped<BookingManagementDomainMessageHandler>();
            
            // Messaging Services (trong MessagingServices folder)
            services.AddScoped<IMessagingService, MessagingService>();
            
            // Handler Registry and Background Service
            services.AddSingleton<IHandlerRegistryService, HandlerRegistryService>();
            services.AddHostedService<HandlerRegistrationHostedService>();
            
            return services;
        }
    }
}
