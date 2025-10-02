using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SurveyTalkService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using SurveyTalkService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using SurveyTalkService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace SurveyTalkService.BusinessLogic.Registrations
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
