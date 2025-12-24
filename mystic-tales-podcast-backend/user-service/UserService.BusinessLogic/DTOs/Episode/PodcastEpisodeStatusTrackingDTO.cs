namespace UserService.BusinessLogic.DTOs.Episode
{
    public class PodcastEpisodeStatusTrackingDTO
    {
        public Guid Id { get; set; }

        public Guid PodcastEpisodeId { get; set; }

        public int PodcastEpisodeStatusId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}