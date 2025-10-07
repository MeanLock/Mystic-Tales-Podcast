using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities.sqlserver;

public partial class PodcastSuggestionConfig
{
    public int ConfigProfileId { get; set; }

    public int BehaviorLookbackDayCount { get; set; }

    public int MinChannelQuery { get; set; }

    public int MinShowQuery { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
