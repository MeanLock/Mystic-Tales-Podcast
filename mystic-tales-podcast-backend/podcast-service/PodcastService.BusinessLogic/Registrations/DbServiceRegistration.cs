using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using Microsoft.Extensions.DependencyInjection;
using PodcastService.BusinessLogic.Services.DbServices.FilterServices;
using PodcastService.BusinessLogic.Services.DbServices.SurveyServices;
using PodcastService.BusinessLogic.Services.DbServices.PaymentServices;
using PodcastService.BusinessLogic.Services.DbServices.ReportServices;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;

namespace PodcastService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // ConfigServices
            services.AddScoped<SystemConfigService>();

            // PodcastServices
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
