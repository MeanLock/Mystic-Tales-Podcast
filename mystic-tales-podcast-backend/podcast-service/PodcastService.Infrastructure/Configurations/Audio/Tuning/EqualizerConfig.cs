using Microsoft.Extensions.Configuration;
using PodcastService.Infrastructure.Enums.Audio.Tuning;
using PodcastService.Infrastructure.Models.Audio.Tuning;

namespace PodcastService.Infrastructure.Configurations.Audio.Tuning
{
    public class EqualizerConfigModel
    {
        public int ShortAudioThresholdSeconds { get; set; }
        public int DefaultShortSegmentSeconds { get; set; }
        public int DefaultLongSegmentSeconds { get; set; }
    }
    public class EqualizerConfig : IEqualizerConfig
    {
        public int ShortAudioThresholdSeconds { get; set; }
        public int DefaultShortSegmentSeconds { get; set; }
        public int DefaultLongSegmentSeconds { get; set; }

        public EqualizerConfig(IConfiguration configuration)
        {
            var equalizerConfig = configuration.GetSection("Infrastructure:AudioTuning:Equalizer").Get<EqualizerConfigModel>();
            if (equalizerConfig != null)
            {
                ShortAudioThresholdSeconds = equalizerConfig.ShortAudioThresholdSeconds;
                DefaultShortSegmentSeconds = equalizerConfig.DefaultShortSegmentSeconds;
                DefaultLongSegmentSeconds = equalizerConfig.DefaultLongSegmentSeconds;
            }
        }
    }
                
    
}