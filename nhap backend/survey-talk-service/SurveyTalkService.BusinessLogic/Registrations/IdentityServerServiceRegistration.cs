using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.BusinessLogic.Services.IdentityServerServices;

namespace SurveyTalkService.BusinessLogic.Registrations
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
