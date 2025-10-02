using Microsoft.Extensions.DependencyInjection;
using TransactionService.Common.Registrations;

namespace TransactionService.Common
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
