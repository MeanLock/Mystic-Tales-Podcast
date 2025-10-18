using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.BusinessLogic.Services.IdentityServerServices;

namespace SubscriptionService.BusinessLogic.Registrations
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
