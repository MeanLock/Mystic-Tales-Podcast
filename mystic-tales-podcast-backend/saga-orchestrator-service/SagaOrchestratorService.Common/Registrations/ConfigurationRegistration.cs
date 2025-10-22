using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.Common.AppConfigurations.App;
using SagaOrchestratorService.Common.AppConfigurations.App.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.Bcrypt;
using SagaOrchestratorService.Common.AppConfigurations.Bcrypt.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.FilePath;
using SagaOrchestratorService.Common.AppConfigurations.FilePath.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.Jwt;
using SagaOrchestratorService.Common.AppConfigurations.Jwt.interfaces;
using SagaOrchestratorService.Common.AppConfigurations.Saga;
using SagaOrchestratorService.Common.AppConfigurations.Saga.interfaces;

namespace SagaOrchestratorService.Common.Registrations
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

            // NEW: YAML saga flow singleton
            services.AddSingleton<ISagaFlowConfig, SagaFlowConfig>();

            return services;
        }
    }
}
