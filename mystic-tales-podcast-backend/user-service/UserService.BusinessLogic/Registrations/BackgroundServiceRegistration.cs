using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using UserService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using UserService.BusinessLogic.Services.BackgroundServices.SystemQueryMetricUpdateJobs;
using UserService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace UserService.BusinessLogic.Registrations
{
    public static class BackgroundServiceRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // services.AddHostedService<HourBaseMajorCloseScheduleService>();
            // services.AddHostedService<HourBaseMajorServiceCloseScheduleService>();

            // services.AddHostedService<MinuteBaseRequestCancellationService>();
            // services.AddHostedService<HourBaseRequestCancellationService>();

            services.AddHostedService<PodcasterAllTimeMaxQueryMetricUpdateJob>();
            services.AddHostedService<PodcasterTemporal7dMaxQueryMetricUpdateJob>();
            
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
