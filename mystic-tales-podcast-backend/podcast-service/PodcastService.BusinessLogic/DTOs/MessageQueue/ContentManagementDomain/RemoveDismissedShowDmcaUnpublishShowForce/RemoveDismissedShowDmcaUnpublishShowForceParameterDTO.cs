namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaUnpublishShowForce
{
    public class RemoveDismissedShowDmcaUnpublishShowForceParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required Guid? DmcaDismissedShowId { get; set; } = null;
    }
}
