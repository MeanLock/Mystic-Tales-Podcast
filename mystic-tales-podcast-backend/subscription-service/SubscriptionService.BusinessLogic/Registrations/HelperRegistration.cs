using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.BusinessLogic.Helpers.AuthHelpers;
using SubscriptionService.BusinessLogic.Helpers.DateHelpers;
using SubscriptionService.BusinessLogic.Helpers.FileHelpers;

namespace SubscriptionService.BusinessLogic.Registrations
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
