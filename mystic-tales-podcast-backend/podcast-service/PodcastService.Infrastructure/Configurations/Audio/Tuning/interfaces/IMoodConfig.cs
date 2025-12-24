using PodcastService.Infrastructure.Enums.Audio.Tuning;
using PodcastService.Infrastructure.Models.Audio.Tuning;

namespace PodcastService.Infrastructure.Configurations.Audio.Tuning
{
    public interface IMoodConfig
    {
        (BaseEqualizerProfile Eq, string Filters)? Get(MoodEnum mood);
    }
}
