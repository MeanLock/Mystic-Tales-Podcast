using Microsoft.Extensions.DependencyInjection;
using SubscriptionService.BusinessLogic.Services.DbServices.MiscServices;
using SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices;

namespace SubscriptionService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // SubscriptionServices
            services.AddScoped<PodcastSubscriptionService>();
            services.AddScoped<MemberSubscriptionService>();

            // MiscServices
            services.AddScoped<MailOperationService>();

            // CachingServices
            services.AddScoped<AccountCachingService>();



            return services;
        }
    }
}
