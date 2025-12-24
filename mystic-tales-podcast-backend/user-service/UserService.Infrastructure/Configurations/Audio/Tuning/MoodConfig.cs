using Microsoft.Extensions.Configuration;
using UserService.Infrastructure.Enums.Audio.Tuning;
using UserService.Infrastructure.Models.Audio.Tuning;

namespace UserService.Infrastructure.Configurations.Audio.Tuning
{
    public class MoodConfig : IMoodConfig
    {
        private readonly Dictionary<MoodEnum, (BaseEqualizerProfile Eq, string Filters)> _cache = new();

        public MoodConfig(IConfiguration configuration)
        {
            var root = configuration.GetSection("Infrastructure:AudioTuning:Mood");
            foreach (var moodSection in root.GetChildren())
            {
                if (!Enum.TryParse<MoodEnum>(moodSection.Key, true, out var moodEnum))
                    continue;

                var eqSec = moodSection.GetSection("EqBands");
                var eq = new BaseEqualizerProfile
                {
                    SubBass = eqSec.GetValue<int?>("SubBass") ?? 0,
                    Bass = eqSec.GetValue<int?>("Bass") ?? 0,
                    Low = eqSec.GetValue<int?>("Low") ?? 0,
                    LowMid = eqSec.GetValue<int?>("LowMid") ?? 0,
                    Mid = eqSec.GetValue<int?>("Mid") ?? 0,
                    Presence = eqSec.GetValue<int?>("Presence") ?? 0,
                    HighMid = eqSec.GetValue<int?>("HighMid") ?? 0,
                    Treble = eqSec.GetValue<int?>("Treble") ?? 0,
                    Air = eqSec.GetValue<int?>("Air") ?? 0
                };

                var filters = (moodSection.GetValue<string?>("Filters") ?? string.Empty).Trim();
                _cache[moodEnum] = (eq, filters);
            }
        }

        public (BaseEqualizerProfile Eq, string Filters)? Get(MoodEnum mood)
        {
            return _cache.TryGetValue(mood, out var value)
                ? value
                : ((BaseEqualizerProfile Eq, string Filters)?)null;
        }
    }
}