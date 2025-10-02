using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Services.MessagingServices.interfaces;
using TransactionService.BusinessLogic.Services.MessagingServices;
using TransactionService.BusinessLogic.MessageHandlers;
using TransactionService.BusinessLogic.Services.DbServices.TransactionServices;

namespace TransactionService.BusinessLogic.Registrations
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
