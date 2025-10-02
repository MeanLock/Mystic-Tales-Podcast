using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Seeders;
using ModerationService.DataAccess.Seeders.IdentityServer;

namespace ModerationService.DataAccess.Registrations
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
