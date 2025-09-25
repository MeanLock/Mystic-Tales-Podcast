using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class AccountLevelSettingConfig
{
    public int ConfigProfileId { get; set; }

    public int Level { get; set; }

    public int DailyReductionXp { get; set; }

    public int ProgressionSurveyCount { get; set; }

    public double BonusRate { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
