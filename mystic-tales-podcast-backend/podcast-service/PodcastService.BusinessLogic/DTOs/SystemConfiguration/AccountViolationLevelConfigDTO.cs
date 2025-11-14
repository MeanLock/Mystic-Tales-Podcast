using System;
using System.Collections.Generic;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Episode;

namespace PodcastService.BusinessLogic.DTOs.SystemConfiguration
{
    public class AccountViolationLevelConfigDTO
    {
        public int ConfigProfileId { get; set; }

        public int ViolationLevel { get; set; }

        public int ViolationPointThreshold { get; set; }

        public int PunishmentDays { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }

}

