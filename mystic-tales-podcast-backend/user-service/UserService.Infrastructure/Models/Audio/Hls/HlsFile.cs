namespace UserService.Infrastructure.Models.Audio.Hls
{
    /// <summary>
    /// Represents a single HLS file (playlist or segment)
    /// </summary>
    public class HlsFile
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public HlsFileType FileType { get; set; }
        public long FileSize { get; set; }
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }
}

