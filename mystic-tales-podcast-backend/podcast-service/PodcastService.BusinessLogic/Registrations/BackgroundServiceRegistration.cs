using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodcastService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using PodcastService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using PodcastService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

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
            
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
