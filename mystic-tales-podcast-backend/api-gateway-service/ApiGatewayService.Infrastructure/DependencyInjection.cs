using Microsoft.Extensions.DependencyInjection;
using ApiGatewayService.Infrastructure.Registrations;
using Microsoft.Extensions.Configuration;

namespace ApiGatewayService.Infrastructure
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
