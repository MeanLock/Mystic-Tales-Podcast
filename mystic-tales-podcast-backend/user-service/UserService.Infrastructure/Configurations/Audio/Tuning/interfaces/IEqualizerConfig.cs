using UserService.Infrastructure.Enums.Audio.Tuning;
using UserService.Infrastructure.Models.Audio.Tuning;

namespace UserService.Infrastructure.Configurations.Audio.Tuning
{
    public interface IEqualizerConfig
    {
        int ShortAudioThresholdSeconds { get; set; }
        int DefaultShortSegmentSeconds { get; set; }
        int DefaultLongSegmentSeconds { get; set; }
    }
}
