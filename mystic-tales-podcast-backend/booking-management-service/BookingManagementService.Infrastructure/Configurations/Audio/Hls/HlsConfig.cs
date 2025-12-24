using Microsoft.Extensions.Configuration;
using BookingManagementService.Infrastructure.Configurations.Audio.Hls.interfaces;

namespace BookingManagementService.Infrastructure.Configurations.Audio.Hls
{
    public class HlsConfigModel
    {
        public int ShortAudioThresholdSeconds { get; set; }
        public int DefaultShortSegmentSeconds { get; set; }
        public int DefaultLongSegmentSeconds { get; set; }
        public string FfmpegPath { get; set; } = string.Empty;
        public string PlaylistFileName { get; set; } = string.Empty;
        public string SegmentFileNamePattern { get; set; } = string.Empty;
        public HlsEncryptionConfigModel Encryption { get; set; } = new();
    }

    public class HlsEncryptionConfigModel
    {
        public bool Enabled { get; set; }
        public string KeyFileName { get; set; } = string.Empty;
        public string KeyInfoFileName { get; set; } = string.Empty;
    }

    public class HlsEncryptionConfig : IHlsEncryptionConfig
    {
        public bool Enabled { get; set; }
        public string KeyFileName { get; set; } = string.Empty;
        public string KeyInfoFileName { get; set; } = string.Empty;

        public HlsEncryptionConfig(HlsEncryptionConfigModel model)
        {
            Enabled = model.Enabled;
            KeyFileName = model.KeyFileName;
            KeyInfoFileName = model.KeyInfoFileName;
        }
    }

    public class HlsConfig : IHlsConfig
    {
        public int ShortAudioThresholdSeconds { get; set; }
        public int DefaultShortSegmentSeconds { get; set; }
        public int DefaultLongSegmentSeconds { get; set; }
        public string FfmpegPath { get; set; } = string.Empty;
        public string PlaylistFileName { get; set; } = string.Empty;
        public string SegmentFileNamePattern { get; set; } = string.Empty;
        public IHlsEncryptionConfig Encryption { get; set; } = new HlsEncryptionConfig(new HlsEncryptionConfigModel());

        public HlsConfig(IConfiguration configuration)
        {
            var hlsConfig = configuration.GetSection("Infrastructure:Hls").Get<HlsConfigModel>();
            if (hlsConfig != null)
            {
                ShortAudioThresholdSeconds = hlsConfig.ShortAudioThresholdSeconds;
                DefaultShortSegmentSeconds = hlsConfig.DefaultShortSegmentSeconds;
                DefaultLongSegmentSeconds = hlsConfig.DefaultLongSegmentSeconds;
                FfmpegPath = hlsConfig.FfmpegPath;
                PlaylistFileName = hlsConfig.PlaylistFileName;
                SegmentFileNamePattern = hlsConfig.SegmentFileNamePattern;
                Encryption = new HlsEncryptionConfig(hlsConfig.Encryption);
            }
        }
    }
}
