using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyTagFilter
{
    public int SurveyId { get; set; }

    public int FilterTagId { get; set; }

    public string? Summary { get; set; }

    public virtual FilterTag FilterTag { get; set; } = null!;

    public virtual Survey Survey { get; set; } = null!;
}
