using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Seeders;
using SurveyTalkService.DataAccess.Seeders.IdentityServer;

namespace SurveyTalkService.DataAccess.Registrations
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
