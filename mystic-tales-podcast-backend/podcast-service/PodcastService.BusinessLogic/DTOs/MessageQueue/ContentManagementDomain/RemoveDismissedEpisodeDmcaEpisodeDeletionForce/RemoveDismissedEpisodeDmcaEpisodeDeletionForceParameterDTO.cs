namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaEpisodeDeletionForce
{
    public class RemoveDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required Guid DmcaDismissedEpisodeId { get; set; }
    }
}
