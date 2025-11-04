namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteChannelEpisodesListenSessionUnpublishChannelForce
{
    public class DeleteChannelEpisodesListenSessionUnpublishChannelForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
