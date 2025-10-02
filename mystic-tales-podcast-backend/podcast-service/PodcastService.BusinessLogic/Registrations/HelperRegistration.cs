using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.DateHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;

namespace PodcastService.BusinessLogic.Registrations
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
