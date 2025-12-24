namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodePublishRequestDTO
    {
        public required EpisodePublishInfoDTO EpisodePublishInfo { get; set; }
    }

    public class EpisodePublishInfoDTO
    {
        public DateOnly? ReleaseDate { get; set; }
    }
}