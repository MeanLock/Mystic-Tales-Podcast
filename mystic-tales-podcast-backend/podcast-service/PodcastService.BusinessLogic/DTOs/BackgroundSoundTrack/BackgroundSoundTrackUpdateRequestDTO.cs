using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.BackgroundSoundTrack
{
    public class BackgroundSoundTrackUpdateRequestDTO
    {
        public required string BackgroundSoundTrackUpdateInfo { get; set; }
        public IFormFile? MainImageFile { get; set; }
        public IFormFile? AudioFile { get; set; }
    }
    public class BackgroundSoundTrackUpdateInfoDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
