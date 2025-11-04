namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedShowsDmcaChannelDeletionForce
{
    public class RemoveChannelDismissedShowsDmcaChannelDeletionForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedShowIds { get; set; }
    }
}
