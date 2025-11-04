namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaShowDeletionForce
{
    public class RemoveDismissedShowDmcaShowDeletionForceParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required Guid DmcaDismissedShowId { get; set; }
    }
}
