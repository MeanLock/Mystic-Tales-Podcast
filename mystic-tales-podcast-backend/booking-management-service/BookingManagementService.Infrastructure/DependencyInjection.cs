using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.Infrastructure.Registrations;

namespace BookingManagementService.Infrastructure
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
