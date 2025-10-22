using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.Common.Registrations;

namespace SystemConfigurationService.Common
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
