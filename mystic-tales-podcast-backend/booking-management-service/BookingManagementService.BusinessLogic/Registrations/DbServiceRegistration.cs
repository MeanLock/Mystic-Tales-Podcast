using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.BusinessLogic.Services.DbServices.MiscServices;
using BookingManagementService.BusinessLogic.Services.DbServices.BookingServices;

namespace BookingManagementService.BusinessLogic.Registrations
{
    public static class DbServiceRegistration
    {
        public static IServiceCollection AddDbServices(this IServiceCollection services)
        {
            // MiscServices
            services.AddScoped<MailOperationService>();

            // BookingServices
            services.AddScoped<BookingService>(); 
            services.AddScoped<BookingProducingRequestService>();
            services.AddScoped<ChatRoomService>();

            return services;
        }
    }
}
