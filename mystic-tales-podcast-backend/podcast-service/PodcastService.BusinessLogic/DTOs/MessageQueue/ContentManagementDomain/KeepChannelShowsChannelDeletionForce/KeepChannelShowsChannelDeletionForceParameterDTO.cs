namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.KeepChannelShowsChannelDeletionForce
{
    public class KeepChannelShowsChannelDeletionForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> KeptShowIds { get; set; }
    }
}
