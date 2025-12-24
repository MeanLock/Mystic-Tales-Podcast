using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.BusinessLogic.Registrations;

namespace BookingManagementService.BusinessLogic
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
