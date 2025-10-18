using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemConfigurationService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using SystemConfigurationService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using SystemConfigurationService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using SystemConfigurationService.BusinessLogic.Services.CrossServiceServices;
using SystemConfigurationService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
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
