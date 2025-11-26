using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.Podcast
{
    public class PodcastChannelWithShowDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? BackgroundImageFileKey { get; set; }
        public string? MainImageFileKey { get; set; }
        public int TotalFavorite { get; set; }
        public int ListenCount { get; set; }
        public int PodcasterId { get; set; }
        public int? PodcastCategoryId { get; set; }
        public int? PodcastSubCategoryId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PodcastChannelStatusTrackingDTO>? PodcastChannelStatusTrackings { get; set; }
        public List<PodcastShowDTO>? PodcastShows { get; set; }
    }
}
