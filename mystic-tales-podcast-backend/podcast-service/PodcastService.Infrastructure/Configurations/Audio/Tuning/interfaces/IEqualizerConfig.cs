using PodcastService.Infrastructure.Enums.Audio.Tuning;
using PodcastService.Infrastructure.Models.Audio.Tuning;

namespace PodcastService.Infrastructure.Configurations.Audio.Tuning
{
    public interface IEqualizerConfig
    {
        int ShortAudioThresholdSeconds { get; set; }
        int DefaultShortSegmentSeconds { get; set; }
        int DefaultLongSegmentSeconds { get; set; }
    }
}
