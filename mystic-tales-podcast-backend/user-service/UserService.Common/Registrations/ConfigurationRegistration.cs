using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UserService.Common.AppConfigurations.App;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.Common.AppConfigurations.Bcrypt;
using UserService.Common.AppConfigurations.Bcrypt.interfaces;
using UserService.Common.AppConfigurations.FilePath;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.Common.AppConfigurations.Jwt;
using UserService.Common.AppConfigurations.Jwt.interfaces;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.Common.AppConfigurations.BusinessSetting;
using UserService.Common.AppConfigurations.Media;
using UserService.Common.AppConfigurations.Media.interfaces;
using UserService.Common.AppConfigurations.SystemService;
using UserService.Common.AppConfigurations.SystemService.interfaces;

namespace UserService.Common.Registrations
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
            services.AddSingleton<IBackgroundJobsConfig, BackgroundJobsConfig>();
            services.AddSingleton<ICustomerListenSessionProcedureConfig, CustomerListenSessionProcedureConfig>();

            // SystemService
            services.AddSingleton<ISystemServiceConfig,SystemServiceConfig>();
 
            return services;
        }
    }
}
