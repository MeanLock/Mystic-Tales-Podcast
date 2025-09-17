using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Architecture_1.BusinessLogic.Services.MessagingServices.interfaces;
using Architecture_1.BusinessLogic.Services.MessagingServices;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityServices;
using Architecture_1.BusinessLogic.MessageHandlers;

namespace Architecture_1.BusinessLogic.Registrations
{
    public static class MessagingServiceRegistration
    {
        public static IServiceCollection AddMessagingServices(this IServiceCollection services)
        {
            // Message Handlers
            services.AddScoped<FacilityMessageHandler>();
            
            // Messaging Services (trong MessagingServices folder)
            services.AddScoped<IMessagingService, MessagingService>();
            
            // Handler Registry and Background Service
            services.AddSingleton<IHandlerRegistryService, HandlerRegistryService>();
            services.AddHostedService<HandlerRegistrationHostedService>();
            
            // Facility Messaging Service
            services.AddScoped<FacilityMessagingService>();
            
            return services;
        }
    }
}
