using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using TransactionService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using TransactionService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using TransactionService.BusinessLogic.Services.CrossServiceServices;
using TransactionService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace TransactionService.BusinessLogic.Registrations
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
