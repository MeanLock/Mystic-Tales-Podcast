namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishShow
{
    public class PublishShowParameterDTO
    {
        public required int PodcasterId { get; set; }
        public required Guid PodcastShowId { get; set; }
        public DateOnly? ReleaseDate { get; set; }
    }
}
