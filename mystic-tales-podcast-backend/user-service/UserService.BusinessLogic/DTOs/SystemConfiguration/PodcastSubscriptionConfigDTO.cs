using System;
using System.Collections.Generic;
using UserService.BusinessLogic.DTOs.Channel;
using UserService.BusinessLogic.DTOs.Episode;

namespace UserService.BusinessLogic.DTOs.SystemConfiguration
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

