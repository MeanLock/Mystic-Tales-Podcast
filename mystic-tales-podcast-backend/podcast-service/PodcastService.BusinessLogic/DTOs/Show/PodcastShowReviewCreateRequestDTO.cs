namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class PodcastShowReviewCreateRequestDTO
    {
        public required PodcastShowReviewCreateInfoDTO PodcastShowReviewCreateInfo { get; set; } 
    }

    public class PodcastShowReviewCreateInfoDTO
    {
        public string? Title { get; set; } = null;
        public string? Content { get; set; } = null;
        public required float Rating { get; set; }
    }
}