using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SubscriptionService.Common.AppConfigurations.App;
using SubscriptionService.Common.AppConfigurations.App.interfaces;
using SubscriptionService.Common.AppConfigurations.Bcrypt;
using SubscriptionService.Common.AppConfigurations.Bcrypt.interfaces;
using SubscriptionService.Common.AppConfigurations.FilePath;
using SubscriptionService.Common.AppConfigurations.FilePath.interfaces;
using SubscriptionService.Common.AppConfigurations.Jwt;
using SubscriptionService.Common.AppConfigurations.Jwt.interfaces;
using SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces;
using SubscriptionService.Common.AppConfigurations.BusinessSetting;
using SubscriptionService.Common.AppConfigurations.Media;
using SubscriptionService.Common.AppConfigurations.Media.interfaces;
using SubscriptionService.Common.AppConfigurations.SystemService;
using SubscriptionService.Common.AppConfigurations.SystemService.interfaces;

namespace SubscriptionService.Common.Registrations
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
