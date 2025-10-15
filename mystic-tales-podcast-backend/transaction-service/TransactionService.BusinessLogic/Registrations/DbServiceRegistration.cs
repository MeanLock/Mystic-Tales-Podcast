using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Services.DbServices;
using TransactionService.BusinessLogic.Services.DbServices.MiscServices;

namespace TransactionService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // // ConfigServices
            // services.AddScoped<SystemConfigService>();

            // // TransactionServices
            // services.AddScoped<AuthService>();
            // services.AddScoped<AccountService>();

            // PaymentServices
            services.AddScoped<AccountBalanceTransactionService>();
            services.AddScoped<BookingTransactionService>();

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



            return services;
        }
    }
}
