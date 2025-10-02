using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.Common.Registrations;

namespace BookingManagementService.Common
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
