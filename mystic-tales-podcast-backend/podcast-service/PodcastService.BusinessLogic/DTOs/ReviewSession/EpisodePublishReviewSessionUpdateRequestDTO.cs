using PodcastService.BusinessLogic.DTOs.Episode;

namespace PodcastService.BusinessLogic.DTOs.ReviewSession
{
    public class EpisodePublishReviewSessionUpdateRequestDTO
    {
        public EpisodePublishReviewSessionUpdateInfoDTO EpisodePublishReviewSessionUpdateInfo { get; set; } = null!;
    }

    public class EpisodePublishReviewSessionUpdateInfoDTO
    {
        public string? Note { get; set; }
        public List<int> PodcastIllegalContentTypeIds { get; set; } = null!;

    }
}