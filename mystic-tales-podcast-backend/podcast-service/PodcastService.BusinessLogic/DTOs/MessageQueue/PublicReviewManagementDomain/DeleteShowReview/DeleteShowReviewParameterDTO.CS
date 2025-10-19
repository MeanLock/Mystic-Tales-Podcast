namespace PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReview
{
    public class DeleteShowReviewParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastShowReviewId { get; set; }
    }
}
