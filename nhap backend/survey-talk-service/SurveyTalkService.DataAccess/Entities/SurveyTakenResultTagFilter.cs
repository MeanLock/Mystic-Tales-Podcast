using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyTakenResultTagFilter
{
    public int SurveyTakenResultId { get; set; }

    public int AdditionalFilterTagId { get; set; }

    public string? Summary { get; set; }

    public virtual FilterTag AdditionalFilterTag { get; set; } = null!;

    public virtual SurveyTakenResult SurveyTakenResult { get; set; } = null!;
}
