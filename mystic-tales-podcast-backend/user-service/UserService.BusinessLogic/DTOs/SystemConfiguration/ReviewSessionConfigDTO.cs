using System;
using System.Collections.Generic;
using UserService.BusinessLogic.DTOs.Channel;
using UserService.BusinessLogic.DTOs.Episode;

namespace UserService.BusinessLogic.DTOs.SystemConfiguration
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

