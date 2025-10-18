using Microsoft.Extensions.DependencyInjection;
using ApiGatewayService.Common.AppConfigurations.App;
using ApiGatewayService.Common.AppConfigurations.App.interfaces;
using ApiGatewayService.Common.AppConfigurations.Jwt;
using ApiGatewayService.Common.AppConfigurations.Jwt.interfaces;

namespace ApiGatewayService.Common.Registrations
{
    public static class ConfigurationRegistration
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            // APP
            services.AddSingleton<IAppConfig, AppConfig>();

            // JWT
            services.AddSingleton<IJwtConfig, JwtConfig>();
 
            return services;
        }
    }
}
