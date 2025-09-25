using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class AccountGeneralConfig
{
    public int ConfigProfileId { get; set; }

    public int XpLevelThreshold { get; set; }

    public int FilterSurveyCycle { get; set; }

    public int DailyActiveCountPeriod { get; set; }

    public double SafetyFilterRate { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
