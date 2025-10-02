using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.BusinessLogic.Services.MessagingServices.interfaces;
using UserService.BusinessLogic.Services.MessagingServices;
using UserService.BusinessLogic.MessageHandlers;
using UserService.BusinessLogic.Services.DbServices.UserServices;

namespace UserService.BusinessLogic.Registrations
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
