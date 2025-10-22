using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace PodcastService.Infrastructure.Models.Audio.Tuning
{
    public class BackgroundMergeProfile
    {
        public double? VolumeGainDb { get; set; } = null;
        [JsonIgnore]
        [BindNever]
        public Stream? FileStream { get; set; } = null;
    }
}
