namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowEpisodesDmcaUnpublishShowForce
{
    public class RemoveDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
