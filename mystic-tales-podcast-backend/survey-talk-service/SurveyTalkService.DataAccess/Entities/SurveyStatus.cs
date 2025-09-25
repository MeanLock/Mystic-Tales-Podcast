using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<SurveyMarketVersionStatusTracking> SurveyMarketVersionStatusTrackings { get; set; } = new List<SurveyMarketVersionStatusTracking>();

    public virtual ICollection<SurveyStatusTracking> SurveyStatusTrackings { get; set; } = new List<SurveyStatusTracking>();
}
