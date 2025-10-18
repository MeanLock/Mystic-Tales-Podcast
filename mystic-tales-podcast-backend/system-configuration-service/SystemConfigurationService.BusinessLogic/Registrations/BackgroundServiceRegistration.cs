using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemConfigurationService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using SystemConfigurationService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using SystemConfigurationService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
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
