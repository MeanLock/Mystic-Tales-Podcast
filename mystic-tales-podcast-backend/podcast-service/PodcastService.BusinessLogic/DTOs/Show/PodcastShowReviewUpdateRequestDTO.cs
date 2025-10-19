namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class PodcastShowReviewUpdateRequestDTO
    {
        public required PodcastShowReviewUpdateInfoDTO PodcastShowReviewUpdateInfo { get; set; }
    }

    public class PodcastShowReviewUpdateInfoDTO
    {
        public string? Title { get; set; } = null;
        public string? Content { get; set; } = null;
        public required float Rating { get; set; }
    }
}