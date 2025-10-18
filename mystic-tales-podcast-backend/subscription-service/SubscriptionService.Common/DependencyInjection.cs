using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.Common.Registrations;

namespace SubscriptionService.Common
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
