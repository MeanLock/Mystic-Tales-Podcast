using Microsoft.Extensions.DependencyInjection;
using PodcastService.Common.Registrations;

namespace PodcastService.Common
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
