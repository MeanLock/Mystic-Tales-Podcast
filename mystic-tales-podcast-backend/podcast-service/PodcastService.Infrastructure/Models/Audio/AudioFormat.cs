namespace PodcastService.Infrastructure.Models.Audio
{
    /// <summary>
    /// Supported audio formats
    /// </summary>
    public enum AudioFormat
    {
        Unknown = 0,
        MP3 = 1,
        AAC = 2,
        M4A = 3,
        FLAC = 4,
        WAV = 5
    }

    /// <summary>
    /// Audio format information with codec details
    /// </summary>
    public class AudioFormatInfo
    {
        /// <summary>
        /// Detected audio format
        /// </summary>
        public AudioFormat Format { get; set; } = AudioFormat.Unknown;

        /// <summary>
        /// File extension (e.g., ".mp3", ".flac")
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// FFmpeg codec name (e.g., "libmp3lame", "flac", "aac")
        /// </summary>
        public string FfmpegCodec { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a lossless format
        /// </summary>
        public bool IsLossless { get; set; } = false;

        /// <summary>
        /// Recommended bitrate for lossy formats (in kbps)
        /// </summary>
        public int? RecommendedBitrate { get; set; }

        /// <summary>
        /// MIME type
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Whether format is supported
        /// </summary>
        public bool IsSupported => Format != AudioFormat.Unknown;
    }
}