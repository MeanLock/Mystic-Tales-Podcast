using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace PodcastService.Infrastructure.Models.Audio.Tuning
{
    public class MultipleTimeRangeBackgroundMergeProfile
    {
        public List<TimeRangeMergeBackground> TimeRangeMergeBackgrounds { get; set; } = new List<TimeRangeMergeBackground>();
    }

    public class TimeRangeMergeBackground
    {
        // Background cut range
        public double BackgroundCutStartSecond { get; set; }
        public double BackgroundCutEndSecond { get; set; }
        
        // Merge position in original audio
        public double OriginalMergeStartSecond { get; set; }
        public double OriginalMergeEndSecond { get; set; }
        public double? VolumeGainDb { get; set; } = null;
        
        [JsonIgnore]
        [BindNever]
        public Stream? FileStream { get; set; } = null;
        
        // Helper properties for validation
        [JsonIgnore]
        public double BackgroundDuration => BackgroundCutEndSecond - BackgroundCutStartSecond;
        
        [JsonIgnore]
        public double MergeDuration => OriginalMergeEndSecond - OriginalMergeStartSecond;
        
        public bool IsValid()
        {
            return BackgroundCutEndSecond > BackgroundCutStartSecond 
                && OriginalMergeEndSecond > OriginalMergeStartSecond
                && Math.Abs(BackgroundDuration - MergeDuration) < 0.01; // tolerance 10ms
        }
    }
}