namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeletePodcastBuddyReview
{
    public class DeletePodcastBuddyReviewParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastBuddyReviewId { get; set; }
    }
}

