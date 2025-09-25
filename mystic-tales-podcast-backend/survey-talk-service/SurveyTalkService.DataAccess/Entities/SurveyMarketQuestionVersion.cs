using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyMarketQuestionVersion
{
    public int SurveyQuestionId { get; set; }

    public byte Version { get; set; }

    public bool IsReanswerRequired { get; set; }

    public int? ReferenceSurveyQuestionId { get; set; }

    public virtual SurveyQuestion? ReferenceSurveyQuestion { get; set; }

    public virtual SurveyQuestion SurveyQuestion { get; set; } = null!;
}
