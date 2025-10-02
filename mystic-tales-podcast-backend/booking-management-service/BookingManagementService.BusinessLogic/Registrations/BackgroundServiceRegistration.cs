using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

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
            
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
