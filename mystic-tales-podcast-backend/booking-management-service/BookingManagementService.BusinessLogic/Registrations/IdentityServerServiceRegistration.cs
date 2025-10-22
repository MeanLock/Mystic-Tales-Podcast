using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.BusinessLogic.Services.IdentityServerServices;

namespace BookingManagementService.BusinessLogic.Registrations
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
