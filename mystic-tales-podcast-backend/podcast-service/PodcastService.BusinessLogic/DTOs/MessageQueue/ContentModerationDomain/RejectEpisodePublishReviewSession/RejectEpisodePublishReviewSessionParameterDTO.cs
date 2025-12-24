namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RejectEpisodePublishReviewSession
{
    public class RejectEpisodePublishReviewSessionParameterDTO
    {
        public required int PodcastEpisodePublishReviewSessionId { get; set; }
        public string? Note { get; set; }
        public required List<int> PodcastIllegalContentTypeIds { get; set; }
        public required int StaffId { get; set; }
    }
}
