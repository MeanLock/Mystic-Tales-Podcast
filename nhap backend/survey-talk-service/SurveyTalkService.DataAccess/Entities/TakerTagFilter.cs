using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class TakerTagFilter
{
    public int TakerId { get; set; }

    public int FilterTagId { get; set; }

    public string? Summary { get; set; }

    public virtual FilterTag FilterTag { get; set; } = null!;

    public virtual Account Taker { get; set; } = null!;
}
