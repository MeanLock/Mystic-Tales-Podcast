using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Infrastructure.Registrations;

namespace TransactionService.Infrastructure
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
