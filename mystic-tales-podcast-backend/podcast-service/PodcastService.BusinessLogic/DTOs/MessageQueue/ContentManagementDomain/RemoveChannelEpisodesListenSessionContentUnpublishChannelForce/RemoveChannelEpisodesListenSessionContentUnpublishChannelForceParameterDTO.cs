namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelEpisodesListenSessionContentUnpublishChannelForce
{
    public class RemoveChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
