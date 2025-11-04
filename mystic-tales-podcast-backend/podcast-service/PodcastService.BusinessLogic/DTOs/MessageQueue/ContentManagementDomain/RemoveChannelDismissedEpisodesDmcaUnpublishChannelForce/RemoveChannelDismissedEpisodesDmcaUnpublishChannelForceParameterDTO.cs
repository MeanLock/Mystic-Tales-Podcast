namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedEpisodesDmcaUnpublishChannelForce
{
    public class RemoveChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
