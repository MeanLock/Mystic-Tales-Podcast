using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Registrations;

namespace TransactionService.BusinessLogic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            services.AddBackgroundServices();
            services.AddDbServices();    
            services.AddHelpers();
            services.AddIdentityServerServices();
            services.AddSignalRHubServices();
            services.AddMessagingServices();
            services.AddCrossServiceServices();
            return services;
        }
    }
}
