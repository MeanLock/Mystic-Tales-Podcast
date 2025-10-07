using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities.sqlserver;

public partial class AccountViolationLevelConfig
{
    public int ConfigProfileId { get; set; }

    public int ViolationLevel { get; set; }

    public int ViolationPointThreshold { get; set; }

    public int PunishmentDays { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
