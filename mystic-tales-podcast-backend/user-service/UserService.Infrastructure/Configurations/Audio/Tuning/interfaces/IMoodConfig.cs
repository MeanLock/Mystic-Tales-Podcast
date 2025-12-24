using UserService.Infrastructure.Enums.Audio.Tuning;
using UserService.Infrastructure.Models.Audio.Tuning;

namespace UserService.Infrastructure.Configurations.Audio.Tuning
{
    public interface IMoodConfig
    {
        (BaseEqualizerProfile Eq, string Filters)? Get(MoodEnum mood);
    }
}
