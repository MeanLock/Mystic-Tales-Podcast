namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesListenSessionContentUnpublishShowForce
{
    public class RemoveShowEpisodesListenSessionContentUnpublishShowForceParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
