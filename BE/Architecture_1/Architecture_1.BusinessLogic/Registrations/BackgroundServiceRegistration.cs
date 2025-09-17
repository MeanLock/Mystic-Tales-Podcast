using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Architecture_1.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using Architecture_1.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using Architecture_1.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace Architecture_1.BusinessLogic.Registrations
{
    public static class BackgroundServiceRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<HourBaseMajorCloseScheduleService>();
            services.AddHostedService<HourBaseMajorServiceCloseScheduleService>();

            services.AddHostedService<MinuteBaseRequestCancellationService>();
            services.AddHostedService<HourBaseRequestCancellationService>();
            
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
