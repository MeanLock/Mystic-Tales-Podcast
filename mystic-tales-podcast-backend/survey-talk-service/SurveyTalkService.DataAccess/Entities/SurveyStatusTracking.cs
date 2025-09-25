using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyStatusTracking
{
    public int Id { get; set; }

    public int SurveyId { get; set; }

    public int SurveyStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Survey Survey { get; set; } = null!;

    public virtual SurveyStatus SurveyStatus { get; set; } = null!;
}
