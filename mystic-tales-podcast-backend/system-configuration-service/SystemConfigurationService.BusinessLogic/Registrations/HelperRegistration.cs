using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.BusinessLogic.Helpers.AuthHelpers;
using SystemConfigurationService.BusinessLogic.Helpers.DateHelpers;
using SystemConfigurationService.BusinessLogic.Helpers.FileHelpers;

namespace SystemConfigurationService.BusinessLogic.Registrations
{
    public static class HelperRegistration
    {
        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            // Auth Helpers
            services.AddSingleton<BcryptHelper>();
            services.AddSingleton<JwtHelper>();

            // File Helpers
            services.AddScoped<FileIOHelper>();
            services.AddSingleton<LocalBinaryFileHelper>();
            services.AddSingleton<LocalBase64FileHelper>();
            services.AddSingleton<FilePathHelper>();

            // Date Helpers
            services.AddSingleton<DateHelper>();


            return services;
        }


    }
}
