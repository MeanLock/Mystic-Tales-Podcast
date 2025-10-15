using Microsoft.Extensions.DependencyInjection;
using UserService.BusinessLogic.Services.DbServices.MiscServices;
using UserService.BusinessLogic.Services.DbServices.UserServices;

namespace UserService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // // UserServices
            services.AddScoped<AuthService>();
            services.AddScoped<AccountService>();

            // MiscServices
            services.AddScoped<MailOperationService>();

            return services;
        }
    }
}
