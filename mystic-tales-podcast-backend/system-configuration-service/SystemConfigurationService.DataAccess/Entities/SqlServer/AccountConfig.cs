using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities.sqlserver;

public partial class AccountConfig
{
    public int ConfigProfileId { get; set; }

    public int ViolationPointDecayHours { get; set; }

    public int PodcastListenSlotThreshold { get; set; }

    public int PodcastListenSlotRecoverySeconds { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
