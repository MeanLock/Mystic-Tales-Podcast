using ApiGatewayService.Common.Registrations;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGatewayService.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonLayer(this IServiceCollection services)
        {
            services.AddConfiguration();
            return services;
        }
    }
}
