using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.DataAccess.Repositories;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using SurveyTalkService.DataAccess.UOW;

namespace SurveyTalkService.DataAccess.Registrations
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
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountOnlineTrackingRepository, AccountOnlineTrackingRepository>();
            services.AddScoped<ISurveyRepository, SurveyRepository>();
            services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
            services.AddScoped<ISurveyTopicFavoriteRepository, SurveyTopicFavoriteRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<ISurveyTakenResultRepository, SurveyTakenResultRepository>();
            services.AddScoped<ISurveyStatusTrackingRepository, SurveyStatusTrackingRepository>();
            services.AddScoped<IFilterTagRepository, FilterTagRepository>();
            services.AddScoped<ITakerTagFilterRepository, TakerTagFilterRepository>();
            services.AddScoped<ISystemConfigProfileRepository, SystemConfigProfileRepository>();
            services.AddScoped<ISurveyTakerSegmentRepository, SurveyTakerSegmentRepository>();
            services.AddScoped<ISurveyTagFilterRepository, SurveyTagFilterRepository>();
            services.AddScoped<IAccountProfileRepository, AccountProfileRepository>();
            services.AddScoped<IAccountBalanceTransactionRepository, AccountBalanceTransactionRepository>();
            services.AddScoped<ISurveyCommunityTransactionRepository, SurveyCommunityTransactionRepository>();

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
