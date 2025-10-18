using Microsoft.Extensions.DependencyInjection;
using ModerationService.Common.Registrations;

namespace ModerationService.Common
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
