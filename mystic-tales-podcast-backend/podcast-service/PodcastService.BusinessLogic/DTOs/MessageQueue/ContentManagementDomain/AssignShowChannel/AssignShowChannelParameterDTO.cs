namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.AssignShowChannel
{
    public class AssignShowChannelParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required Guid? PodcastChannelId { get; set; }
        public required int PodcasterId { get; set; }
    }
}
