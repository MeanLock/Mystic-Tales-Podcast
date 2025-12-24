using System;
using System.Collections.Generic;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Episode;

namespace PodcastService.BusinessLogic.DTOs.SystemConfiguration
{
    public class PodcastSubscriptionConfigDTO
    {
        public int ConfigProfileId { get; set; }

        public int SubscriptionCycleTypeId { get; set; }

        public double ProfitRate { get; set; }

        public int IncomeTakenDelayDays { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }

}

