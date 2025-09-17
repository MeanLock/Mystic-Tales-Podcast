using Microsoft.Extensions.DependencyInjection;
using Architecture_1.Common.AppConfigurations.App;
using Architecture_1.Common.AppConfigurations.App.interfaces;
using Architecture_1.Common.AppConfigurations.Bcrypt;
using Architecture_1.Common.AppConfigurations.Bcrypt.interfaces;
using Architecture_1.Common.AppConfigurations.FilePath;
using Architecture_1.Common.AppConfigurations.FilePath.interfaces;
using Architecture_1.Common.AppConfigurations.Jwt;
using Architecture_1.Common.AppConfigurations.Jwt.interfaces;

namespace Architecture_1.Common.Registrations
{
    public static class ConfigurationRegistration
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            // APP
            services.AddSingleton<IAppConfig, AppConfig>();

            // JWT
            services.AddSingleton<IJwtConfig, JwtConfig>();

            // Bcrypt
            services.AddSingleton<IBcryptConfig, BcryptConfig>();

            // FilePath
            services.AddSingleton<IFilePathConfig, FilePathConfig>();

            
            return services;
        }
    }
}
