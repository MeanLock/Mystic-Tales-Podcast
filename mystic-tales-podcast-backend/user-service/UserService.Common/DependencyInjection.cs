using Microsoft.Extensions.DependencyInjection;
using UserService.Common.Registrations;

namespace UserService.Common
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
