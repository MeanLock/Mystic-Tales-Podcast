namespace PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.UpdatePodcastBuddyReview
{
    public class UpdatePodcastBuddyReviewParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastBuddyReviewId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public required float Rating { get; set; }
    }
}
