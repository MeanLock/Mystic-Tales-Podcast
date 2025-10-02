using Microsoft.Extensions.DependencyInjection;
using SagaOrchestratorService.DataAccess.Repositories;
using SagaOrchestratorService.DataAccess.Repositories.interfaces;
using SagaOrchestratorService.DataAccess.UOW;

namespace SagaOrchestratorService.DataAccess.Registrations
{
    public static class RepositoryRegistration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddDbRepositories();
            services.AddGenericRepositories();
            services.AddUnitOfWork();

            return services;
        }

        public static IServiceCollection AddDbRepositories (this IServiceCollection services) {
            return services;
        }

        public static IServiceCollection AddGenericRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }

        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
