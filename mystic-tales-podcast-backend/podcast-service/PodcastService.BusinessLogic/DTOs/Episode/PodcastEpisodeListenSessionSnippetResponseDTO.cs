namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class PodcastEpisodeListenSessionSnippetResponseDTO
    {
        public Guid Id { get; set; }

        public int LastListenDurationSeconds { get; set; }
    }
}