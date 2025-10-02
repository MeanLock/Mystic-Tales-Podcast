using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyMarketConfig
{
    public int ConfigProfileId { get; set; }

    public double DataTransactionProfitRate { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
