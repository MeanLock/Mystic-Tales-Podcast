using Microsoft.Extensions.DependencyInjection;
using Architecture_1.BusinessLogic.Services.IdentityServerServices;
using Duende.IdentityServer.Validation;

namespace Architecture_1.BusinessLogic.Registrations
{
    public static class IdentityServerServiceRegistration
    {
        public static IServiceCollection AddIdentityServerServices(this IServiceCollection services)
        {
            services.AddScoped<IdentityServerConfigurationService>();
            services.AddScoped<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidatorService>();


            return services;
        }
    }
}
