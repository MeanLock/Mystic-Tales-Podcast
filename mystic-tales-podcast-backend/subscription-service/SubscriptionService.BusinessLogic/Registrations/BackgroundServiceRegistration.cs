using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubscriptionService.BusinessLogic.Services.BackgroundServices;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.PodcastSubscriptionJob;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace SubscriptionService.BusinessLogic.Registrations
{
    public static class BackgroundServiceRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<PodcastSubscriptionRegistrationRenewalJob>();
            services.AddHostedService<PodcastSubscriptionIncomeReleaseJob>();
            services.AddHostedService<SampleBackgroundJob>();
            
            services.Configure<HostOptions>(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
            });
            return services;
        }
    }
}
