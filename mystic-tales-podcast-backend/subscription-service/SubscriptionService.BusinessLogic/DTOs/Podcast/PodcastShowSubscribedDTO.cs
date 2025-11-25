using SubscriptionService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Podcast
{
    public class PodcastShowSubscribedDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public DateOnly? ReleaseDate { get; set; }
        public bool? IsReleased { get; set; }
        public string Copyright { get; set; } = null!;
        public string? UploadFrequency { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public string? MainImageFileKey { get; set; }
        public string? TrailerAudioFileKey { get; set; }
        public int TotalFollow { get; set; }
        public int ListenCount { get; set; }
        public int PodcasterId { get; set; }
        public int? PodcastCategoryId { get; set; }
        public int? PodcastSubCategoryId { get; set; }
        public int PodcastShowSubscriptionTypeId { get; set; }
        public Guid? PodcastChannelId { get; set; }
        public string? TakenDownReason { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PodcastCategoryDTO? PodcastCategory { get; set; }
        public PodcastSubCategoryDTO? PodcastSubCategory { get; set; }
        public List<HashtagDTO>? Hashtags { get; set; }
        public PodcastChannelSnippetResponseDTO PodcastChannel { get; set; }
        public PodcastShowSubscriptionTypeDTO PodcastShowSubscriptionType { get; set; }
        public List<PodcastShowStatusTrackingDTO> PodcastShowStatusTrackings { get; set; }
        public List<PodcastEpisodeDTO> PodcastEpisodes { get; set; }
    }
}
