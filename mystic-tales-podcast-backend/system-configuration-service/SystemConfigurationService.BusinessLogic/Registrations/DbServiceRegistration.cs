using SystemConfigurationService.BusinessLogic.Services.DbServices.SystemConfigurationServices;
using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.BusinessLogic.Services.DbServices.FilterServices;
using SystemConfigurationService.BusinessLogic.Services.DbServices.SurveyServices;
using SystemConfigurationService.BusinessLogic.Services.DbServices.PaymentServices;
using SystemConfigurationService.BusinessLogic.Services.DbServices.ReportServices;
using SystemConfigurationService.BusinessLogic.Services.DbServices.MiscServices;

namespace SystemConfigurationService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // ConfigServices
            services.AddScoped<SystemConfigService>();

            // SystemConfigurationServices
            services.AddScoped<AuthService>();
            services.AddScoped<AccountService>();

            // PaymentServices
            services.AddScoped<AccountPaymentService>();

            // SurveyServices
            services.AddScoped<SurveyCoreService>();
            services.AddScoped<SurveySessionService>();
            services.AddScoped<SurveyResponseService>();
            services.AddScoped<SurveyTransactionService>();

            // FilterServices
            services.AddScoped<FilterTagService>();

            // ReportServices
            services.AddScoped<SurveyStatisticsService>();
            services.AddScoped<TransactionStatisticsService>();
            services.AddScoped<UserStatisticsService>();

            // MiscServices
            services.AddScoped<PlatformFeedbackService>();




            return services;
        }
    }
}
