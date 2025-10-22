using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodcastService.DataAccess.Registrations;

namespace PodcastService.DataAccess
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)

        {
            services.AddDbContext(configuration);
            services.AddDuendeIdentityServer(configuration);
            services.AddRepositories();
            services.AddSeeders();
            return services;
        }
    }
}
