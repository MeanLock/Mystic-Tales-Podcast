using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Hashtag;

namespace PodcastService.BusinessLogic.DTOs.Show.ListItems
{
    public class ShowListItemResponseDTO
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
        public required PodcastShowStatusDTO CurrentStatus { get; set; } = null!;
    }
}