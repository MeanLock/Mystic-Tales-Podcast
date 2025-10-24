namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class PodcastEpisodeSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? MainImageFileKey { get; set; }
    }
}