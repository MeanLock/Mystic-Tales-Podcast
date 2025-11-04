namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteShowEpisodesListenSessionUnpublishShowForce
{
    public class DeleteShowEpisodesListenSessionUnpublishShowForceParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
