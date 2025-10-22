using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveySecurityModeConfig
{
    public int ConfigProfileId { get; set; }

    public int SurveySecurityModeId { get; set; }

    public double Rate { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;

    public virtual SurveySecurityMode SurveySecurityMode { get; set; } = null!;
}
