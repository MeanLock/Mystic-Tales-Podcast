using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveySecurityMode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<SurveySecurityModeConfig> SurveySecurityModeConfigs { get; set; } = new List<SurveySecurityModeConfig>();

    public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();
}
