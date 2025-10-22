using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities;

public partial class SurveyMarketResponseVersion
{
    public int SurveyResponseId { get; set; }

    public byte Version { get; set; }

    public virtual SurveyResponse SurveyResponse { get; set; } = null!;
}
