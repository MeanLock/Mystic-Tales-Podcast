using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices;
using SubscriptionService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SubscriptionService.BusinessLogic.Registrations
{
    public static class CrossServiceServiceRegistration
    {
        public static IServiceCollection AddCrossServiceServices(this IServiceCollection services)
        {
            services.AddScoped<HttpServiceQueryClient>();
            services.AddScoped<FieldSelector>();
            services.AddScoped<GenericQueryService>();
            return services;
        }
    }
}
