using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities.SqlServer;

public partial class PodcastSuggestionConfig
{
    public int ConfigProfileId { get; set; }

    public int MinMediumRangeUserBehaviorLookbackDayCount { get; set; }

    public int MinChannelQuery { get; set; }

    public int MinShowQuery { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int MinShortRangeUserBehaviorLookbackDayCount { get; set; }

    public int MinLongRangeUserBehaviorLookbackDayCount { get; set; }

    public int MinShortRangeContentBehaviorLookbackDayCount { get; set; }

    public int MinMediumRangeContentBehaviorLookbackDayCount { get; set; }

    public int MinLongRangeContentBehaviorLookbackDayCount { get; set; }

    public int MinExtraLongRangeContentBehaviorLookbackDayCount { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
