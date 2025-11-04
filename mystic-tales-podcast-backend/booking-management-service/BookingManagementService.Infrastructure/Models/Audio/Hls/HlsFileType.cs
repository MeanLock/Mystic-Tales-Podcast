namespace BookingManagementService.Infrastructure.Models.Audio.Hls
{
    /// <summary>
    /// Types of HLS files
    /// </summary>
    public enum HlsFileType
    {
        Playlist,       // .m3u8 file
        Segment,        // .ts file
        EncryptionKey   // .key file
    }
}

