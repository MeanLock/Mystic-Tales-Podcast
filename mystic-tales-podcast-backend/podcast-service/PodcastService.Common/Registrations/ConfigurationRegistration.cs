using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PodcastService.Common.AppConfigurations.App;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.Bcrypt;
using PodcastService.Common.AppConfigurations.Bcrypt.interfaces;
using PodcastService.Common.AppConfigurations.FilePath;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.Common.AppConfigurations.Jwt;
using PodcastService.Common.AppConfigurations.Jwt.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting;
using PodcastService.Common.AppConfigurations.Media;
using PodcastService.Common.AppConfigurations.Media.interfaces;
using PodcastService.Common.AppConfigurations.SystemService;
using PodcastService.Common.AppConfigurations.SystemService.interfaces;

namespace PodcastService.Common.Registrations
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
            services.AddSingleton<IPodcastPublishReviewSessionConfig, PodcastPublishReviewSessionConfig>();
            services.AddSingleton<IPodcastListenSessionConfig, PodcastListenSessionConfig>();

            // SystemService
            services.AddSingleton<ISystemServiceConfig,SystemServiceConfig>();
 
            return services;
        }
    }
}
