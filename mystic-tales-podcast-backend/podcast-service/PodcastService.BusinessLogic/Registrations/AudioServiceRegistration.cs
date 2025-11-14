using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodcastService.BusinessLogic.Services.AudioServices;

namespace PodcastService.BusinessLogic.Registrations
{
    public static class AudioServiceRegistration
    {
        public static IServiceCollection AddAudioServices(this IServiceCollection services)
        {
            services.AddScoped<AudioTranscriptionService>();
            services.AddScoped<AudioTuningService>();
            return services;
        }
    }
}
