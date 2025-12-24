using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Hashtag;

namespace PodcastService.BusinessLogic.DTOs.Channel.ListItems
{
    public class ChannelListItemResponseDTO
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

        public required PodcastChannelStatusDTO CurrentStatus { get; set; } = null!;

    }
}
