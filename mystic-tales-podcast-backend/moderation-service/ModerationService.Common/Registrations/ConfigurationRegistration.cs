using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ModerationService.Common.AppConfigurations.App;
using ModerationService.Common.AppConfigurations.App.interfaces;
using ModerationService.Common.AppConfigurations.Bcrypt;
using ModerationService.Common.AppConfigurations.Bcrypt.interfaces;
using ModerationService.Common.AppConfigurations.FilePath;
using ModerationService.Common.AppConfigurations.FilePath.interfaces;
using ModerationService.Common.AppConfigurations.Jwt;
using ModerationService.Common.AppConfigurations.Jwt.interfaces;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;
using ModerationService.Common.AppConfigurations.BusinessSetting;
using ModerationService.Common.AppConfigurations.Media;
using ModerationService.Common.AppConfigurations.Media.interfaces;
using ModerationService.Common.AppConfigurations.SystemService;
using ModerationService.Common.AppConfigurations.SystemService.interfaces;

namespace ModerationService.Common.Registrations
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
            services.AddSingleton<IDMCAAccusationConfig, DMCAAccusationConfig>();
            services.AddSingleton<IBackgroundJobsConfig, BackgroundJobsConfig>();

            // SystemService
            services.AddSingleton<ISystemServiceConfig,SystemServiceConfig>();
 
            return services;
        }
    }
}
