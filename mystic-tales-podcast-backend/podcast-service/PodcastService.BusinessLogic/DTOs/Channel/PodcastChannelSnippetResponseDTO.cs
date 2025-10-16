namespace PodcastService.BusinessLogic.DTOs.Channel
{
    public class PodcastChannelSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? MainImageFileKey { get; set; }
    }
}