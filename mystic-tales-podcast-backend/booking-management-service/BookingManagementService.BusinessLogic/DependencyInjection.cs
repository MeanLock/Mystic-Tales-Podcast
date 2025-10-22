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
            services.AddEmbeddingVectorServices();
            services.AddOpenAIServices();
            services.AddMessagingServices();
            services.AddCrossServiceServices();
            return services;
        }
    }
}
