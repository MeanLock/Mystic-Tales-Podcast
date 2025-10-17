using System;
using System.Collections.Generic;

namespace UserService.BusinessLogic.DTOs.Channel
{
    public class PodcastChannelDTO
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
    }

}

