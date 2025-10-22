using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Services.DbServices.DMCAServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;

namespace ModerationService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // ReportServices
            services.AddScoped<PodcastBuddyReportService>();
            services.AddScoped<PodcastShowReportService>();
            services.AddScoped<PodcastEpisodeReportService>();
            services.AddScoped<DMCAAccusationService>();
            services.AddScoped<DMCANoticeService>();
            services.AddScoped<CounterNoticeService>();
            services.AddScoped<LawsuitProofService>();

            // MiscServices
            services.AddScoped<PlatformFeedbackService>();
            services.AddScoped<MailOperationService>();

            // CachingServices
            services.AddScoped<AccountCachingService>();



            return services;
        }
    }
}
