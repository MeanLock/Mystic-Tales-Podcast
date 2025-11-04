namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaUnpublishEpisodeForce
{
    public class RemoveDismissedEpisodeDmcaUnpublishEpisodeForceParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required Guid DmcaDismissedEpisodeId { get; set; }
    }
}
