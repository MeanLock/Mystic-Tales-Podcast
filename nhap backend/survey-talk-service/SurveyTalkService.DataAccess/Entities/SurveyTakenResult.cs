using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyTakenResult
{
    public int Id { get; set; }

    public int SurveyId { get; set; }

    public int TakerId { get; set; }

    public bool IsValid { get; set; }

    public string? InvalidReason { get; set; }

    public decimal? MoneyEarned { get; set; }

    public int? XpEarned { get; set; }

    public DateTime CompletedAt { get; set; }

    public virtual Survey Survey { get; set; } = null!;

    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();

    public virtual ICollection<SurveyTakenResultTagFilter> SurveyTakenResultTagFilters { get; set; } = new List<SurveyTakenResultTagFilter>();

    public virtual Account Taker { get; set; } = null!;
}
