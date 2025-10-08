using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SystemConfigurationService.Common.AppConfigurations.App;
using SystemConfigurationService.Common.AppConfigurations.App.interfaces;
using SystemConfigurationService.Common.AppConfigurations.Bcrypt;
using SystemConfigurationService.Common.AppConfigurations.Bcrypt.interfaces;
using SystemConfigurationService.Common.AppConfigurations.FilePath;
using SystemConfigurationService.Common.AppConfigurations.FilePath.interfaces;
using SystemConfigurationService.Common.AppConfigurations.Jwt;
using SystemConfigurationService.Common.AppConfigurations.Jwt.interfaces;
using SystemConfigurationService.Common.AppConfigurations.BusinessSetting.interfaces;
using SystemConfigurationService.Common.AppConfigurations.BusinessSetting;
using SystemConfigurationService.Common.AppConfigurations.Media;
using SystemConfigurationService.Common.AppConfigurations.Media.interfaces;
using SystemConfigurationService.Common.AppConfigurations.SystemService;
using SystemConfigurationService.Common.AppConfigurations.SystemService.interfaces;

namespace SystemConfigurationService.Common.Registrations
{
    public static class ConfigurationRegistration
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            // APP
            services.AddSingleton<IAppConfig, AppConfig>();

            // Media
            services.AddSingleton<IMediaTypeConfig, MediaTypeConfig>();

            // JWT
            services.AddSingleton<IJwtConfig, JwtConfig>();

            // Bcrypt
            services.AddSingleton<IBcryptConfig, BcryptConfig>();

            // FilePath
            services.AddSingleton<IFilePathConfig, FilePathConfig>();

            // BusinessSetting
            services.AddSingleton<ISurveyConfig, SurveyConfig>();
            services.AddSingleton<IEmbeddingVectorModelConfig, EmbeddingVectorModelConfig>();
            services.AddSingleton<IAccountConfig, AccountConfig>();
            services.AddSingleton<IFileValidationConfig, FileValidationConfig>();
            services.AddSingleton<IMailPropertiesConfig, MailPropertiesConfig>();

            // SystemService
            services.AddSingleton<ISystemServiceConfig,SystemServiceConfig>();
 
            return services;
        }
    }
}
