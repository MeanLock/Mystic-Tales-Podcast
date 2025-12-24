namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishEpisode
{
    public class PublishEpisodeParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required int PodcasterId { get; set; }
        public DateOnly? ReleaseDate { get; set; }
    }
}
