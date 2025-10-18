using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.IdentityServerServices;

namespace PodcastService.BusinessLogic.Registrations
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
