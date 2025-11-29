using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodcastService.BusinessLogic.Services.BackgroundServices.PodcastJobs;
using PodcastService.BusinessLogic.Services.BackgroundServices.SystemQueryMetricUpdateJobs;

namespace PodcastService.BusinessLogic.Registrations
{
    public static class BackgroundServiceRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // services.AddHostedService<HourBaseMajorCloseScheduleService>();
            // services.AddHostedService<HourBaseMajorServiceCloseScheduleService>();

            // services.AddHostedService<MinuteBaseRequestCancellationService>();
            // services.AddHostedService<HourBaseRequestCancellationService>();

            // services.AddHostedService<PodcasterAllTimeMaxQueryMetricUpdateJob>();
            // services.AddHostedService<PodcasterTemporal7dMaxQueryMetricUpdateJob>();
            // services.AddHostedService<ShowAllTimeMaxQueryMetricUpdateJob>();
            // services.AddHostedService<ChannelAllTimeMaxQueryMetricUpdateJob>();
            // services.AddHostedService<EpisodeAllTimeMaxQueryMetricUpdateJob>();
            // services.AddHostedService<ShowTemporal7dMaxQueryMetricUpdateJob>();
            // services.AddHostedService<ChannelTemporal7dMaxQueryMetricUpdateJob>();
            // services.AddHostedService<SystemPreferencesTemporal30dQueryMetricUpdateJob>();
            // services.AddHostedService<UserPreferencesTemporal30dQueryMetricUpdateJob>();

            services.AddHostedService<ShowPublishReleaseJob>();
            services.AddHostedService<EpisodePublishReleaseJob>();
            services.AddHostedService<EpisodeEditRequirementPendingTimeoutJob>();
            services.AddHostedService<EpisodeListenSessionExpiryJob>();
            
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
