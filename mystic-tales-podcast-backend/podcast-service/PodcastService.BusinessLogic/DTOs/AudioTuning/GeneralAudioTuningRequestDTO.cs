using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;
using PodcastService.Infrastructure.Models.Audio.Tuning;

namespace PodcastService.BusinessLogic.DTOs.AudioTuning
{
    public class GeneralAudioTuningRequestDTO
    {
        public string GeneralTuningProfileRequestInfo { get; set; } 
        public IFormFile? AudioFile { get; set; }
    }

    public class GeneralTuningProfileRequestInfo
    {
        public EqualizerProfileRequestInfo? EqualizerProfile { get; set; } = null;
        public BackgroundMergeProfileRequestInfo? BackgroundMergeProfile { get; set; } = null;
        public MultipleTimeRangeBackgroundMergeProfileRequestInfo? MultipleTimeRangeBackgroundMergeProfile { get; set; } = null;
        public AITuningProfileRequestInfo? AITuningProfile { get; set; } = null;
    }

    public class EqualizerProfileRequestInfo
    {
        public ExpandEqualizerProfile? ExpandEqualizer { get; set; } = null;
        public BaseEqualizerProfile? BaseEqualizer { get; set; } = null;
    }

    public class BackgroundMergeProfileRequestInfo
    {
        public double? VolumeGainDb { get; set; } = null;
        public string? BackgroundSoundTrackFileKey { get; set; } = null;
    }

    public class MultipleTimeRangeBackgroundMergeProfileRequestInfo
    {
        public List<TimeRangeMergeBackgroundInfo> TimeRangeMergeBackgrounds { get; set; } = new List<TimeRangeMergeBackgroundInfo>();
    }

    public class TimeRangeMergeBackgroundInfo
    {
        // Background cut range
        public double BackgroundCutStartSecond { get; set; }
        public double BackgroundCutEndSecond { get; set; }

        // Merge position in original audio
        public double OriginalMergeStartSecond { get; set; }
        public double OriginalMergeEndSecond { get; set; }
        public double? VolumeGainDb { get; set; } = null;
        public string? BackgroundSoundTrackFileKey { get; set; } = null;
    }

    public class AITuningProfileRequestInfo
    {
        public UvrMdxNetMainProfile? UvrMdxNetMainProfile { get; set; } = null;
    }



}