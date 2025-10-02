using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using BookingManagementService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace BookingManagementService.BusinessLogic.Registrations
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
