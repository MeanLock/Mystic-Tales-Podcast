using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.BookingJobs;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.PodcastJobs;

namespace BookingManagementService.BusinessLogic.Registrations
{
    public static class BackgroundServiceRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // services.AddHostedService<HourBaseMajorCloseScheduleService>();
            // services.AddHostedService<HourBaseMajorServiceCloseScheduleService>();

            // services.AddHostedService<MinuteBaseRequestCancellationService>();
            // services.AddHostedService<HourBaseRequestCancellationService>();

            services.AddHostedService<SampleBackgroundJob>();
            services.AddHostedService<BookingProducingRequestedResponseTimeoutJob>();
            services.AddHostedService<BookingTrackPreviewingResponseTimeoutJob>();
            services.AddHostedService<BookingPodcastTrackListenSessionExpiryJob>();


            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
