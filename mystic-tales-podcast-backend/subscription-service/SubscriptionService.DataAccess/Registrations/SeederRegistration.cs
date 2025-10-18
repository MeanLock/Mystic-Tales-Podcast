using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.Seeders;
using SubscriptionService.DataAccess.Seeders.IdentityServer;

namespace SubscriptionService.DataAccess.Registrations
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
