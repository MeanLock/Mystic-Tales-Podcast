using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SurveyTalkService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using SurveyTalkService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using SurveyTalkService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;
using SurveyTalkService.BusinessLogic.Services.CrossServiceServices;
using SurveyTalkService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace SurveyTalkService.BusinessLogic.Registrations
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
