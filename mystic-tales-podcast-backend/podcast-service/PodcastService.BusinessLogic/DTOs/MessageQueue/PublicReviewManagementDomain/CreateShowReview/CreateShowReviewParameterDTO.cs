namespace PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.CreateShowReview
{
    public class CreateShowReviewParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastShowId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public required float Rating { get; set; }
    }
}
