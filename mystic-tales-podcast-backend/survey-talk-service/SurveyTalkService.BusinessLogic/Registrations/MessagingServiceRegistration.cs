using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.BusinessLogic.Services.MessagingServices.interfaces;
using SurveyTalkService.BusinessLogic.Services.MessagingServices;
using SurveyTalkService.BusinessLogic.MessageHandlers;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;

namespace SurveyTalkService.BusinessLogic.Registrations
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
