using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModerationService.BusinessLogic.Services.BackgroundServices;
using ModerationService.BusinessLogic.Services.BackgroundServices.DMCAJobs;
using ModerationService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using ModerationService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using ModerationService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace ModerationService.BusinessLogic.Registrations
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
            services.AddHostedService<DMCANoticeResponseTimeoutJob>();
            services.AddHostedService<CounterNoticeResponseTimeoutJob>();

            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
