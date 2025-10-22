using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyTakerSegment
{
    public int SurveyId { get; set; }

    public string? CountryRegion { get; set; }

    public string? MaritalStatus { get; set; }

    public string? AverageIncome { get; set; }

    public string? EducationLevel { get; set; }

    public string? JobField { get; set; }

    public string? Prompt { get; set; }

    public double? TagFilterAccuracyRate { get; set; }

    public virtual Survey Survey { get; set; } = null!;
}
