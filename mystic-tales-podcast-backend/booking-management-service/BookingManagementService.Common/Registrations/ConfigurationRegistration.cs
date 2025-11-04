using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BookingManagementService.Common.AppConfigurations.App;
using BookingManagementService.Common.AppConfigurations.App.interfaces;
using BookingManagementService.Common.AppConfigurations.Bcrypt;
using BookingManagementService.Common.AppConfigurations.Bcrypt.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.Common.AppConfigurations.Jwt;
using BookingManagementService.Common.AppConfigurations.Jwt.interfaces;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using BookingManagementService.Common.AppConfigurations.BusinessSetting;
using BookingManagementService.Common.AppConfigurations.Media;
using BookingManagementService.Common.AppConfigurations.Media.interfaces;
using BookingManagementService.Common.AppConfigurations.SystemService;
using BookingManagementService.Common.AppConfigurations.SystemService.interfaces;

namespace BookingManagementService.Common.Registrations
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

            // SystemService
            services.AddSingleton<ISystemServiceConfig,SystemServiceConfig>();
 
            return services;
        }
    }
}
