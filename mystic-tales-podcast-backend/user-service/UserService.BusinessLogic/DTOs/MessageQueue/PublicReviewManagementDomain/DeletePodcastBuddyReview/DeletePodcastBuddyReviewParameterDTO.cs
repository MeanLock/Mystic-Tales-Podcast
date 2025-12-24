namespace UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeletePodcastBuddyReview
{
    public class DeletePodcastBuddyReviewParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastBuddyReviewId { get; set; }
    }
}

