using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.DbServices.CachingServices;
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
            services.AddScoped<ReviewSessionService>();
            services.AddScoped<PodcastBackgroundSoundTrackService>();
            services.AddScoped<PodcastCategoryService>();
            services.AddScoped<HashtagService>();

            // MiscServices
            services.AddScoped<PlatformFeedbackService>();
            services.AddScoped<MailOperationService>();
            services.AddScoped<QueryMetricCachingService>();
            services.AddScoped<FeedService>();
            

            // CachingServices
            services.AddScoped<AccountCachingService>();
            services.AddScoped<CustomerListenSessionProcedureCachingService>();



            return services;
        }
    }
}
