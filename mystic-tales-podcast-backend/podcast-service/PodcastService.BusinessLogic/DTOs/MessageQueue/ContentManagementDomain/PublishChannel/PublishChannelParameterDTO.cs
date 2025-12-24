namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishChannel
{
    public class PublishChannelParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required int PodcasterId { get; set; }
    }
}
