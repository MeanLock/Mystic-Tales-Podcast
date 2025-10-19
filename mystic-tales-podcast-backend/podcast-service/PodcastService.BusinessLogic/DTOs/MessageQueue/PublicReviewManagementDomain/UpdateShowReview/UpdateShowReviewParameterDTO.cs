namespace PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.UpdateShowReview
{
    public class UpdateShowReviewParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastShowReviewId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public required float Rating { get; set; }
    }
}
