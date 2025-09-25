using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyGeneralConfig
{
    public int ConfigProfileId { get; set; }

    public decimal PricePerQuestion { get; set; }

    public int XpPerQuestion { get; set; }

    public double PublishProfitRate { get; set; }

    public double BasePriceAllocationRate { get; set; }

    public double TimePriceAllocationRate { get; set; }

    public double LevelPriceAllocationRate { get; set; }

    public virtual SystemConfigProfile ConfigProfile { get; set; } = null!;
}
