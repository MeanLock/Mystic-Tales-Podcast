using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Seeders;
using BookingManagementService.DataAccess.Seeders.IdentityServer;

namespace BookingManagementService.DataAccess.Registrations
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
