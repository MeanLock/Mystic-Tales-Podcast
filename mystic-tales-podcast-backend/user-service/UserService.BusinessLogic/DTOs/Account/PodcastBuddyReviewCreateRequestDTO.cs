namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcastBuddyReviewCreateRequestDTO
    {
        public required PodcastBuddyReviewCreateInfoDTO PodcastBuddyReviewCreateInfo { get; set; }
    }

    public class PodcastBuddyReviewCreateInfoDTO
    {
        public string? Title { get; set; } = null;
        public string? Content { get; set; } = null;
        public required float Rating { get; set; }
    }
}