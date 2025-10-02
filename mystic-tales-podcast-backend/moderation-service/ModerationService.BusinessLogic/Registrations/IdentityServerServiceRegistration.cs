using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Services.IdentityServerServices;

namespace ModerationService.BusinessLogic.Registrations
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
