using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.BackgroundSoundTrack
{
    public class PodcastBackgroundSoundTrackDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? MainImageFileKey { get; set; }

        public string? AudioFileKey { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
