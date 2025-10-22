using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PodcastService.BusinessLogic.Services.AudioServices;
using PodcastService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices;
using PodcastService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using PodcastService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices;

namespace PodcastService.BusinessLogic.Registrations
{
    public static class AudioServiceRegistration
    {
        public static IServiceCollection AddAudioServices(this IServiceCollection services)
        {
            services.AddScoped<AudioTranscriptionService>();
            return services;
        }
    }
}
