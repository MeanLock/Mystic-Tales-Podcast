namespace UserService.BusinessLogic.DTOs.Show
{
    public class PodcastShowStatusTrackingDTO
    {
        public Guid Id { get; set; }

        public Guid PodcastShowId { get; set; }

        public int PodcastShowStatusId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}