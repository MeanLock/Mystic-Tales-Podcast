using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Services.IdentityServerServices;

namespace TransactionService.BusinessLogic.Registrations
{
    public static class IdentityServerServiceRegistration
    {
        public static IServiceCollection AddIdentityServerServices(this IServiceCollection services)
        {
            services.AddScoped<IdentityServerConfigurationService>();


            return services;
        }
    }
}
