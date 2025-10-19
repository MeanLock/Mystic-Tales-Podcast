namespace UserService.BusinessLogic.DTOs.Channel
{
    public class PodcastChannelStatusTrackingDTO
    {
        public Guid Id { get; set; }

        public Guid PodcastChannelId { get; set; }

        public int PodcastChannelStatusId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}