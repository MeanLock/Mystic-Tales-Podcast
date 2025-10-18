using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.DataAccess.Registrations;

namespace UserService.DataAccess
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
