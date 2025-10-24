namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.AcceptEpisodePublishReviewSession
{
    public class AcceptEpisodePublishReviewSessionParameterDTO
    {
        public required int PodcastEpisodePublishReviewSessionId { get; set; }
        public string? Note { get; set; }
        public required List<int> PodcastIllegalContentTypeIds { get; set; }
        public required int StaffId { get; set; }
    }
}
