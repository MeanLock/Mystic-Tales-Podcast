using System;
using System.Collections.Generic;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Episode;

namespace PodcastService.BusinessLogic.DTOs.SystemConfiguration
{
    public class ReviewSessionConfigDTO
    {
        public int ConfigProfileId { get; set; }

        public int PodcastBuddyUnResolvedReportStreak { get; set; }

        public int PodcastShowUnResolvedReportStreak { get; set; }

        public int PodcastEpisodeUnResolvedReportStreak { get; set; }

        public int PodcastEpisodePublishEditRequirementExpiredHours { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}

