using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.ReviewSession.ListItems;
using PodcastService.BusinessLogic.DTOs.Show;

namespace PodcastService.BusinessLogic.DTOs.ReviewSession.Details
{
    public class EpisodePublishReviewSessionDetailResponseDTO
    {
        public int Id { get; set; }
        public int AssignedStaff { get; set; }
        public string? Note { get; set; }
        public int ReReviewCount { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PodcastEpisodeDTO PodcastEpisode { get; set; } = null!;
        public List<PodcastIllegalContentTypeDTO> PodcastIllegalContentTypeList { get; set; } = null!;
        public List<PodcastEpisodeDTO> PublishDuplicateDetectedPodcastEpisodes { get; set; } = null!;
        public List<string> RestrictedTermFoundList { get; set; } = null!;
        public PodcastChannelSnippetResponseDTO? PodcastChannel { get; set; } = null!;
        public required PodcastShowSnippetResponseDTO PodcastShow { get; set; } = null!;
        public required AccountStatusSnippetDTO Podcaster { get; set; } = null!;
        public required PodcastEpisodeStatusDTO EpisodeCurrentStatus { get; set; } = null!;
        public required PodcastShowStatusDTO ShowCurrentStatus { get; set; } = null!;
        public PodcastChannelStatusDTO? ChannelCurrentStatus { get; set; } = null!;
        public required PodcastEpisodePublishReviewSessionStatusDTO CurrentStatus { get; set; } = null!;


    }
}
