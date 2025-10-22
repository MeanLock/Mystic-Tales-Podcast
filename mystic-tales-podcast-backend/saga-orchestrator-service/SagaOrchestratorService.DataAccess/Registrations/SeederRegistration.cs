using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.DataAccess.Data;
using SagaOrchestratorService.DataAccess.Seeders;
using SagaOrchestratorService.DataAccess.Seeders.IdentityServer;

namespace SagaOrchestratorService.DataAccess.Registrations
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
