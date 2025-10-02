using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.Seeders;
using SystemConfigurationService.DataAccess.Seeders.IdentityServer;

namespace SystemConfigurationService.DataAccess.Registrations
{
    public static class SeederRegistration
    {
        public static IServiceCollection AddSeeders(this IServiceCollection services)
        {
            services.AddTransient<ConfigurationSeeder>();
            return services;
        }
    }
}
