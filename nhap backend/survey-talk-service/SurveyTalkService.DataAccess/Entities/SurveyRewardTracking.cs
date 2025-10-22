using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyRewardTracking
{
    public int Id { get; set; }

    public int SurveyId { get; set; }

    public decimal RewardPrice { get; set; }

    public int RewardXp { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Survey Survey { get; set; } = null!;
}
