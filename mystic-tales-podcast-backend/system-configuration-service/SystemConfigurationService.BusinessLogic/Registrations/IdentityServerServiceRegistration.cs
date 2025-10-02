using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.BusinessLogic.Services.IdentityServerServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
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
