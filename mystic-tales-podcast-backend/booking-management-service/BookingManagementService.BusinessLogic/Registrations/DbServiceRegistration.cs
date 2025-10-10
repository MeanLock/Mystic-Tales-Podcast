using Microsoft.Extensions.DependencyInjection;
using UserService.BusinessLogic.Services.DbServices;
using BookingManagementService.BusinessLogic.Services.DbServices;

namespace BookingManagementService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // // ConfigServices
            // services.AddScoped<SystemConfigService>();

            // // BookingManagementServices
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

            services.AddScoped<BookingService>(); 
            services.AddScoped<BookingProducingRequestService>();

            return services;
        }
    }
}
