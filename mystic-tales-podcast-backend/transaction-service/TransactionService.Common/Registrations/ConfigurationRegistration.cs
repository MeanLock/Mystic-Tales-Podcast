using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TransactionService.Common.AppConfigurations.App;
using TransactionService.Common.AppConfigurations.App.interfaces;
using TransactionService.Common.AppConfigurations.Bcrypt;
using TransactionService.Common.AppConfigurations.Bcrypt.interfaces;
using TransactionService.Common.AppConfigurations.FilePath;
using TransactionService.Common.AppConfigurations.FilePath.interfaces;
using TransactionService.Common.AppConfigurations.Jwt;
using TransactionService.Common.AppConfigurations.Jwt.interfaces;
using TransactionService.Common.AppConfigurations.BusinessSetting.interfaces;
using TransactionService.Common.AppConfigurations.BusinessSetting;
using TransactionService.Common.AppConfigurations.Media;
using TransactionService.Common.AppConfigurations.Media.interfaces;
using TransactionService.Common.AppConfigurations.SystemService;
using TransactionService.Common.AppConfigurations.SystemService.interfaces;

namespace TransactionService.Common.Registrations
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
