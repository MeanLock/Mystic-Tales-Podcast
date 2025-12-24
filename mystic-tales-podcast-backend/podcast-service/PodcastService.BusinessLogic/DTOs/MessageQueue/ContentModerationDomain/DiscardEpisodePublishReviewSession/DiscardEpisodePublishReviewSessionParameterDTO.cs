namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewSession
{
    public class DiscardEpisodePublishReviewSessionParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required int PodcasterId { get; set; }
    }
}
