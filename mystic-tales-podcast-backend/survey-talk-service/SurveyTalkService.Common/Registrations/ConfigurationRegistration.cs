using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SurveyTalkService.Common.AppConfigurations.App;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;
using SurveyTalkService.Common.AppConfigurations.Bcrypt;
using SurveyTalkService.Common.AppConfigurations.Bcrypt.interfaces;
using SurveyTalkService.Common.AppConfigurations.FilePath;
using SurveyTalkService.Common.AppConfigurations.FilePath.interfaces;
using SurveyTalkService.Common.AppConfigurations.Jwt;
using SurveyTalkService.Common.AppConfigurations.Jwt.interfaces;
using SurveyTalkService.Common.AppConfigurations.BusinessSetting.interfaces;
using SurveyTalkService.Common.AppConfigurations.BusinessSetting;
using SurveyTalkService.Common.AppConfigurations.Media;
using SurveyTalkService.Common.AppConfigurations.Media.interfaces;
using SurveyTalkService.Common.AppConfigurations.SystemService;
using SurveyTalkService.Common.AppConfigurations.SystemService.interfaces;

namespace SurveyTalkService.Common.Registrations
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

            // SystemService
            services.AddSingleton<ISystemServiceConfig,SystemServiceConfig>();
 
            return services;
        }
    }
}
