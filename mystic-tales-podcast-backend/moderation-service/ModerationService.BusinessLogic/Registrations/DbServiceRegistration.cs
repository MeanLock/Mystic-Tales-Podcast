using ModerationService.BusinessLogic.Services.DbServices.ModerationServices;
using Microsoft.Extensions.DependencyInjection;
using ModerationService.BusinessLogic.Services.DbServices.FilterServices;
using ModerationService.BusinessLogic.Services.DbServices.SurveyServices;
using ModerationService.BusinessLogic.Services.DbServices.PaymentServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;

namespace ModerationService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // ConfigServices
            services.AddScoped<SystemConfigService>();

            // ModerationServices
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
