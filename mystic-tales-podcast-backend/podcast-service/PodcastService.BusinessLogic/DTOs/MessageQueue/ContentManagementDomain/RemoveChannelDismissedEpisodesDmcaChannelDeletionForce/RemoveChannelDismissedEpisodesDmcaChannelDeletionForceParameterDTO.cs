namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedEpisodesDmcaChannelDeletionForce
{
    public class RemoveChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
