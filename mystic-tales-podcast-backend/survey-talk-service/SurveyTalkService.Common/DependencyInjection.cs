using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.Common.Registrations;

namespace SurveyTalkService.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonLayer(this IServiceCollection services)
        {
            services.AddConfiguration();
            return services;
        }
    }
}
