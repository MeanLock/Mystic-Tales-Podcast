namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class PodcastEpisodeSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? MainImageFileKey { get; set; }
        public required bool? IsReleased { get; set; }
        public required DateOnly? ReleaseDate { get; set; }
    }
}