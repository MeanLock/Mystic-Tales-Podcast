using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodcastService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using PodcastService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using PodcastService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using PodcastService.BusinessLogic.Services.CrossServiceServices;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace PodcastService.BusinessLogic.Registrations
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
