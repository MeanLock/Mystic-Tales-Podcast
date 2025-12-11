namespace PodcastService.Infrastructure.Models.Audio.Tuning
{
    public class GeneralTuningProfile
    {
        public EqualizerProfile? EqualizerProfile { get; set; } = null;
        public BackgroundMergeProfile? BackgroundMergeProfile { get; set; } = null;
        public MultipleTimeRangeBackgroundMergeProfile? MultipleTimeRangeBackgroundMergeProfile { get; set; } = null;
        public AITuningProfile? AITuningProfile { get; set; } = null;
    }
}
