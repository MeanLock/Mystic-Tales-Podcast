namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcastBuddyReviewRequestDTO
    {
        public required PodcastBuddyReviewRequestInfoDTO PodcastBuddyReviewRequestInfo { get; set; }
    }

    public class PodcastBuddyReviewRequestInfoDTO
    {
        public string? Title { get; set; } = null;
        public string? Content { get; set; } = null;
        public required float Rating { get; set; }
    }
}