using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionService.BusinessLogic.Services.BackgroundServices;
using TransactionService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using TransactionService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using TransactionService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace TransactionService.BusinessLogic.Registrations
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
            
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
