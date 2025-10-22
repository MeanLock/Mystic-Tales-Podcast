using Microsoft.Extensions.Configuration;
using UserService.Infrastructure.Configurations.Audio.Hls.interfaces;

namespace UserService.Infrastructure.Configurations.Audio.Hls
{
    public class HlsConfigModel
    {
        public int ShortAudioThresholdSeconds { get; set; }
        public int DefaultShortSegmentSeconds { get; set; }
        public int DefaultLongSegmentSeconds { get; set; }
        public string FfmpegPath { get; set; } = string.Empty;
        public string PlaylistName { get; set; } = string.Empty;
        public string SegmentFilePattern { get; set; } = string.Empty;
        public HlsEncryptionConfigModel Encryption { get; set; } = new();
    }

    public class HlsEncryptionConfigModel
    {
        public bool Enabled { get; set; }
        public string KeyFile { get; set; } = string.Empty;
        public string KeyInfoFile { get; set; } = string.Empty;
    }

    public class HlsEncryptionConfig : IHlsEncryptionConfig
    {
        public bool Enabled { get; set; }
        public string KeyFile { get; set; } = string.Empty;
        public string KeyInfoFile { get; set; } = string.Empty;

        public HlsEncryptionConfig(HlsEncryptionConfigModel model)
        {
            Enabled = model.Enabled;
            KeyFile = model.KeyFile;
            KeyInfoFile = model.KeyInfoFile;
        }
    }

    public class HlsConfig : IHlsConfig
    {
        public int ShortAudioThresholdSeconds { get; set; }
        public int DefaultShortSegmentSeconds { get; set; }
        public int DefaultLongSegmentSeconds { get; set; }
        public string FfmpegPath { get; set; } = string.Empty;
        public string PlaylistName { get; set; } = string.Empty;
        public string SegmentFilePattern { get; set; } = string.Empty;
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
                PlaylistName = hlsConfig.PlaylistName;
                SegmentFilePattern = hlsConfig.SegmentFilePattern;
                Encryption = new HlsEncryptionConfig(hlsConfig.Encryption);
            }
        }
    }
}
