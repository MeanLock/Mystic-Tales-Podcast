using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class PlatformFeedback
{
    public int AccountId { get; set; }

    public double RatingScore { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
