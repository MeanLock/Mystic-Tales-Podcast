using SubscriptionService.BusinessLogic.DTOs.Podcast;
using SubscriptionService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription
{
    public class SubscribedContentResponseDTO
    {
        public List<PodcastChannelSubscribedContentResponseDTO> PodcastChannelList { get; set; }
        public List<PodcastShowSubscribedContentResponseDTO> PodcastShowList { get; set; }
    }
    public class PodcastChannelSubscribedContentResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? BackgroundImageFileKey { get; set; }
        public string? MainImageFileKey { get; set; }
        public int TotalFavorite { get; set; }
        public int ListenCount { get; set; }
        public int ShowCount { get; set; }
        public AccountSnippetResponseDTO Podcaster { get; set; } = null!;
        public PodcastCategoryDTO? PodcastCategory { get; set; }
        public PodcastSubCategoryDTO? PodcastSubCategory { get; set; }
        public List<HashtagDTO>? Hashtags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PodcastStatusDTO CurrentStatus { get; set; } = null!;
    }
    public class PodcastShowSubscribedContentResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public DateOnly? ReleaseDate { get; set; }
        public bool? IsReleased { get; set; }
        public string Copyright { get; set; } = null!;
        public string? UploadFrequency { get; set; }
        public int RatingCount { get; set; }
        public double AverageRating { get; set; }
        public string? MainImageFileKey { get; set; }
        public string? TrailerAudioFileKey { get; set; }
        public int ListenCount { get; set; }
        public int TotalFollow { get; set; }
        public int EpisodeCount { get; set; }
        public AccountSnippetResponseDTO Podcaster { get; set; } = null!;
        public PodcastCategoryDTO? PodcastCategory { get; set; }
        public PodcastSubCategoryDTO? PodcastSubCategory { get; set; }
        public PodcastShowSubscriptionTypeDTO? PodcastShowSubscriptionType { get; set; }
        public PodcastChannelSnippetResponseDTO? PodcastChannel { get; set; }
        public List<HashtagDTO>? Hashtags { get; set; }
        public string? TakenDownReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PodcastStatusDTO CurrentStatus { get; set; } = null!;
    }
    public class PodcastStatusDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
