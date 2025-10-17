using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.BusinessLogic.Services.DbServices.MiscServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // // ConfigServices
            // services.AddScoped<SystemConfigService>();

            // // SystemConfigurationServices
            // services.AddScoped<AuthService>();
            // services.AddScoped<AccountService>();

            // // PaymentServices
            // services.AddScoped<AccountPaymentService>();

            // // SurveyServices
            // services.AddScoped<SurveyCoreService>();
            // services.AddScoped<SurveySessionService>();
            // services.AddScoped<SurveyResponseService>();
            // services.AddScoped<SurveyTransactionService>();

            // // FilterServices
            // services.AddScoped<FilterTagService>();

            // // ReportServices
            // services.AddScoped<SurveyStatisticsService>();
            // services.AddScoped<TransactionStatisticsService>();
            // services.AddScoped<UserStatisticsService>();

            // MiscServices
            services.AddScoped<PlatformFeedbackService>();
            services.AddScoped<MailOperationService>();

            // CachingServices
            services.AddScoped<AccountCachingService>();




            return services;
        }
    }
}
