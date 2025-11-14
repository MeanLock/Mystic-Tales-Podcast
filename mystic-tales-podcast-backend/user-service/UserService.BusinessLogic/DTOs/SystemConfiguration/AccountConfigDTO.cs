using System;
using System.Collections.Generic;
using UserService.BusinessLogic.DTOs.Channel;
using UserService.BusinessLogic.DTOs.Episode;

namespace UserService.BusinessLogic.DTOs.SystemConfiguration
{
    public class AccountConfigDTO
    {
        public int ConfigProfileId { get; set; }

        public int ViolationPointDecayHours { get; set; }

        public int PodcastListenSlotThreshold { get; set; }

        public int PodcastListenSlotRecoverySeconds { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }

}

