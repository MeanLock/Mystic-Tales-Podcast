using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using UserService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using UserService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using UserService.BusinessLogic.Services.CrossServiceServices;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace UserService.BusinessLogic.Registrations
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
