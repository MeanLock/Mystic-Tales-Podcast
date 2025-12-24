namespace PodcastService.Infrastructure.Models.Audio.Transcription
{
    public class AudioTranscriptionApiResult
    {
        public string Transcript { get; set; } = string.Empty;
        public double DurationSeconds { get; set; }
        public long FileSizeBytes { get; set; }
        public List<int> AudioShape { get; set; } = new List<int>();
        public int SampleRate { get; set; }
        public string ModelUsed { get; set; } = string.Empty;
        public string DeviceUsed { get; set; } = string.Empty;
        // public string? MemoryInfo { get; set; } // Nullable if not using CUDA
        public string ProcessingMode { get; set; } = "memory_direct"; // Default to "memory_direct"
    }
}