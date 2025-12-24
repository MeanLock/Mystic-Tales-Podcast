using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Show;

namespace PodcastService.BusinessLogic.DTOs.Episode.ListItems
{
    public class EpisodeListItemResponseDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool ExplicitContent { get; set; }

        public DateOnly? ReleaseDate { get; set; }
        public int EpisodeOrder { get; set; }

        public bool? IsReleased { get; set; }

        public string? MainImageFileKey { get; set; }

        public string? AudioFileKey { get; set; } = null!;

        public double? AudioFileSize { get; set; }

        public int? AudioLength { get; set; }
        public required PodcastEpisodeSubscriptionTypeDTO PodcastEpisodeSubscriptionType { get; set; }

        public required PodcastShowSnippetResponseDTO PodcastShow { get; set; }
        public List<HashtagDTO>? Hashtags { get; set; }

        public int SeasonNumber { get; set; }

        public int TotalSave { get; set; }

        public int ListenCount { get; set; }

        public bool? IsAudioPublishable { get; set; }

        public string? TakenDownReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        public required PodcastEpisodeStatusDTO CurrentStatus { get; set; } = null!;

    }
}