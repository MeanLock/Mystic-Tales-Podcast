namespace UserService.Infrastructure.Configurations.Audio.Hls.interfaces
{
    public interface IHlsConfig
    {
        int ShortAudioThresholdSeconds { get; set; }
        int DefaultShortSegmentSeconds { get; set; }
        int DefaultLongSegmentSeconds { get; set; }
        string FfmpegPath { get; set; }
        string PlaylistName { get; set; }
        string SegmentFilePattern { get; set; }
        IHlsEncryptionConfig Encryption { get; set; }
    }

    public interface IHlsEncryptionConfig
    {
        bool Enabled { get; set; }
        string KeyFile { get; set; }
        string KeyInfoFile { get; set; }
    }
}
