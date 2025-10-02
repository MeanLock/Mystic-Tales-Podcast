using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.BusinessLogic.Services.DbServices.FilterServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.SurveyServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.PaymentServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.ReportServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.MiscServices;

namespace SurveyTalkService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // ConfigServices
            services.AddScoped<SystemConfigService>();

            // UserServices
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
