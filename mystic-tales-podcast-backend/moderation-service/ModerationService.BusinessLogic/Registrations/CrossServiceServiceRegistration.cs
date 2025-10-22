using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModerationService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using ModerationService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using ModerationService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using ModerationService.BusinessLogic.Services.CrossServiceServices;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace ModerationService.BusinessLogic.Registrations
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
