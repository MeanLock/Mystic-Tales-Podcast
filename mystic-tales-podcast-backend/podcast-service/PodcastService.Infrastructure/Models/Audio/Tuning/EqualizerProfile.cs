
using PodcastService.Infrastructure.Enums.Audio.Tuning;

namespace PodcastService.Infrastructure.Models.Audio.Tuning
{
    public class EqualizerProfile
    {
        public ExpandEqualizerProfile? ExpandEqualizer { get; set; } = null;
        public BaseEqualizerProfile? BaseEqualizer { get; set; } = null;
    }
    public class ExpandEqualizerProfile
    {
        public MoodEnum? Mood { get; set; } = null;
    }
    public class BaseEqualizerProfile
    {
        public int? SubBass { get; set; } = null;
        public int? Bass { get; set; } = null;
        public int? Low { get; set; } = null;
        public int? LowMid { get; set; } = null;
        public int? Mid { get; set; } = null;
        public int? Presence { get; set; } = null;
        public int? HighMid { get; set; } = null;
        public int? Treble { get; set; } = null;
        public int? Air { get; set; } = null;
    }
   
}
