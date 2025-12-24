namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcastBuddyReviewUpdateRequestDTO
    {
        public required PodcastBuddyReviewUpdateInfoDTO PodcastBuddyReviewUpdateInfo { get; set; }
    }

    public class PodcastBuddyReviewUpdateInfoDTO
    {
        public string? Title { get; set; } = null;
        public string? Content { get; set; } = null;
        public required float Rating { get; set; }
    }
}