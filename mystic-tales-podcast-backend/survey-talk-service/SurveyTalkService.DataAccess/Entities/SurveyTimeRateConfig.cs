using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyTimeRateConfig
{
    public int Id { get; set; }

    public int ConfigProfileId { get; set; }

    public double MinDurationRate { get; set; }

    public double MaxDurationRate { get; set; }

    public double Rate { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
