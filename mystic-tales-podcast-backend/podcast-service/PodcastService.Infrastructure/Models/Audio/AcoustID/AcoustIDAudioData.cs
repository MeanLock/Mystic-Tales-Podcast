namespace PodcastService.Infrastructure.Models.Audio.AcoustID
{
    public class AcoustIDAudioData
    {
        public short[] Samples { get; set; } = Array.Empty<short>();
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public double Duration { get; set; }
    }
}