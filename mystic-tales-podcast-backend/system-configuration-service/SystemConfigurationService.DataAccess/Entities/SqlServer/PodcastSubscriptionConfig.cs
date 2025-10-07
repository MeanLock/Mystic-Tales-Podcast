using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities.sqlserver;

public partial class PodcastSubscriptionConfig
{
    public int ConfigProfileId { get; set; }

    public int SubscriptionCycleTypeId { get; set; }

    public double ProfitRate { get; set; }

    public int IncomeTakenDelayDays { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
