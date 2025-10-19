using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;

namespace PodcastService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // PodcastServices
            services.AddScoped<PodcastChannelService>();
            services.AddScoped<PodcastShowService>();
            services.AddScoped<PodcastEpisodeService>();

            // MiscServices
            services.AddScoped<PlatformFeedbackService>();
            services.AddScoped<MailOperationService>();

            // CachingServices
            services.AddScoped<AccountCachingService>();



            return services;
        }
    }
}
