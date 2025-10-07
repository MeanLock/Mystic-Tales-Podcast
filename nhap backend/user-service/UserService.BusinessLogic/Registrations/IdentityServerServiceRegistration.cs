using Microsoft.Extensions.DependencyInjection;
using UserService.BusinessLogic.Services.IdentityServerServices;

namespace UserService.BusinessLogic.Registrations
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
