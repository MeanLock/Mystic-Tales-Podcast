using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Episode
{
    public class EpisodeCreateRequestDTO
    {
        public required string EpisodeCreateInfo { get; set; } = null!;
        public IFormFile? MainImageFile { get; set; }
    }
    public class EpisodeCreateInfoDTO
    {
        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool ExplicitContent { get; set; }

        // public DateOnly? ReleaseDate { get; set; }

        // public bool? IsReleased { get; set; }

        // public string? MainImageFileKey { get; set; } 

        // public string AudioFileKey { get; set; } = null!;

        // public double AudioFileSize { get; set; } 

        // public int AudioLength { get; set; } 

        // public byte[]? AudioFingerPrint { get; set; }

        public int PodcastEpisodeSubscriptionTypeId { get; set; }

        public Guid PodcastShowId { get; set; }

        public int SeasonNumber { get; set; }
        public int EpisodeOrder { get; set; }
        public List<int>? HashtagIds { get; set; }

        // public int TotalSave { get; set; }

        // public int ListenCount { get; set; } 

        // public bool? IsAudioPublishable { get; set; }

        // public string? TakenDownReason { get; set; }

        // public DateTime? DeletedAt { get; set; }

        // public DateTime CreatedAt { get; set; }

        // public DateTime UpdatedAt { get; set; }

    }
}