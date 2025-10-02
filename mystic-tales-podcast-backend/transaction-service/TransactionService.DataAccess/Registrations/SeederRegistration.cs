using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.Seeders;
using TransactionService.DataAccess.Seeders.IdentityServer;

namespace TransactionService.DataAccess.Registrations
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
