namespace PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteChannelShowsReviewUnpublishChannelForce
{
    public class DeleteChannelShowsReviewUnpublishChannelForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedShowIds { get; set; }
    }
}

