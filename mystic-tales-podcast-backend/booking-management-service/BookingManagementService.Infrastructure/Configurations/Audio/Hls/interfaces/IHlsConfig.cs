namespace BookingManagementService.Infrastructure.Configurations.Audio.Hls.interfaces
{
    public interface IHlsConfig
    {
        int ShortAudioThresholdSeconds { get; set; }
        int DefaultShortSegmentSeconds { get; set; }
        int DefaultLongSegmentSeconds { get; set; }
        string FfmpegPath { get; set; }
        string PlaylistFileName { get; set; }
        string SegmentFileNamePattern { get; set; }
        IHlsEncryptionConfig Encryption { get; set; }
    }

    public interface IHlsEncryptionConfig
    {
        bool Enabled { get; set; }
        string KeyFileName { get; set; }
        string KeyInfoFileName { get; set; }
    }
}
