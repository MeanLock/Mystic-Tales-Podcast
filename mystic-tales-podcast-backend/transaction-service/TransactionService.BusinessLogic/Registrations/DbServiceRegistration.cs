using Microsoft.Extensions.DependencyInjection;
using TransactionService.BusinessLogic.Services.DbServices.MiscServices;
using TransactionService.BusinessLogic.Services.DbServices.TransactionServices;

namespace TransactionService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // MiscServices
            services.AddScoped<PlatformFeedbackService>();
            services.AddScoped<MailOperationService>();

            // TransactionServices
            services.AddScoped<AccountBalanceTransactionService>();
            services.AddScoped<BookingTransactionService>();
            services.AddScoped<PodcastSubscriptionService>();
            services.AddScoped<MemberSubscriptionService>();

            return services;
        }
    }
}
