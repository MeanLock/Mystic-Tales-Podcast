namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class PodcastShowSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? MainImageFileKey { get; set; }
    }
}