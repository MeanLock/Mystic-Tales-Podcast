using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Helpers.AuthHelpers;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Helpers.FileHelpers;

namespace ModerationService.BusinessLogic.Registrations
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
