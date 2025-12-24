using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;

namespace PodcastService.BusinessLogic.DTOs.ReviewSession.ListItems
{
    public class EpisodePublishReviewSessionListItemResponseDTO
    {
        public int Id { get; set; }

        public AccountSnippetResponseDTO AssignedStaff { get; set; }
        public string? Note { get; set; }

        public int ReReviewCount { get; set; }

        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public PodcastEpisodeSnippetResponseDTO PodcastEpisode { get; set; } = null!;
        public PodcastEpisodePublishReviewSessionStatusDTO CurrentStatus { get; set; } = null!;
    }
}