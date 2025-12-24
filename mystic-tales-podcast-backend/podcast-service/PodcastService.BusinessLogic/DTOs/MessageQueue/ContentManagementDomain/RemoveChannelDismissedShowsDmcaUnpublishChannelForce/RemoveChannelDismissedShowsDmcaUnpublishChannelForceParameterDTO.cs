namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedShowsDmcaUnpublishChannelForce
{
    public class RemoveChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedShowIds { get; set; }
    }
}
