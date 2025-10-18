using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.Infrastructure.Registrations;

namespace SubscriptionService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfiguration();
            services.AddServices(configuration);
            return services;
        }
    }
}
