using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyFeedback
{
    public int SurveyId { get; set; }

    public int TakerId { get; set; }

    public double RatingScore { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Survey Survey { get; set; } = null!;

    public virtual Account Taker { get; set; } = null!;
}
