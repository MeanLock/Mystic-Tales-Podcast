namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class PodcastShowSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? MainImageFileKey { get; set; }
        public required bool? IsReleased { get; set; }
        public required DateOnly? ReleaseDate { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}