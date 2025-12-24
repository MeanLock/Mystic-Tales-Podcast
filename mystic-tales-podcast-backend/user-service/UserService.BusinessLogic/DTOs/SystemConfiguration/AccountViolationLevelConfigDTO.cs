using System;
using System.Collections.Generic;
using UserService.BusinessLogic.DTOs.Channel;
using UserService.BusinessLogic.DTOs.Episode;

namespace UserService.BusinessLogic.DTOs.SystemConfiguration
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

