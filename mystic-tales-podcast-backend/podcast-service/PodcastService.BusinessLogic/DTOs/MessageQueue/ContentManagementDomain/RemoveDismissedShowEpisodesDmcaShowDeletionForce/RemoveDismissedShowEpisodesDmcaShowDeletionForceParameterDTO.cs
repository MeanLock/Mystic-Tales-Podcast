namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowEpisodesDmcaShowDeletionForce
{
    public class RemoveDismissedShowEpisodesDmcaShowDeletionForceParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
