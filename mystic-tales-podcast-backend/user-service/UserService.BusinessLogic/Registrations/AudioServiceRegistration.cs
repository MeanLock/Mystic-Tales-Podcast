using Microsoft.Extensions.DependencyInjection;
using UserService.BusinessLogic.Services.AudioServices;

namespace UserService.BusinessLogic.Registrations
{
    public static class AudioServiceRegistration
    {
        public static IServiceCollection AddAudioServices(this IServiceCollection services)
        {
            services.AddScoped<AudioTuningService>();




            return services;
        }
    }
}
