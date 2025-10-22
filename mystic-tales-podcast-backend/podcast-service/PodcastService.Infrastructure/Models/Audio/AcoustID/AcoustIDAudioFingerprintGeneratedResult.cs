namespace PodcastService.Infrastructure.Models.Audio.AcoustID
{
    public class AcoustIDAudioFingerprintGeneratedResult
    {
        public string FingerprintData { get; set; } = string.Empty;
        public string FingerprintHash { get; set; } = string.Empty;
        public double Duration { get; set; } // thời lượng (seconds)
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public double FileSizeMB { get; set; } // Kích thước file (MB)
        public string Algorithm { get; set; } = string.Empty; // thuật toán sử dụng
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}