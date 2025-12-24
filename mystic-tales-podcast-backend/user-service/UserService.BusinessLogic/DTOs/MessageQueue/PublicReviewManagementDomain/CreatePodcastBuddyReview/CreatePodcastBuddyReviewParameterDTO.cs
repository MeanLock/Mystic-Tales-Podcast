namespace UserService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.CreatePodcastBuddyReview
{
    public class CreatePodcastBuddyReviewParameterDTO
    {
        public required int AccountId { get; set; }
        public required int PodcastBuddyId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public required float Rating { get; set; }
    }
}
