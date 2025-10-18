using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.DataAccess.Data;
using UserService.DataAccess.Seeders;
using UserService.DataAccess.Seeders.IdentityServer;

namespace UserService.DataAccess.Registrations
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
