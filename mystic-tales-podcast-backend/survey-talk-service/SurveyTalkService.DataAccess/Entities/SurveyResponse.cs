using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyResponse
{
    public int Id { get; set; }

    public int SurveyTakenResultId { get; set; }

    public bool IsValid { get; set; }

    public string ValueJsonString { get; set; } = null!;

    public Guid SurveyQuestionId { get; set; }

    public virtual ICollection<SurveyMarketResponseVersion> SurveyMarketResponseVersions { get; set; } = new List<SurveyMarketResponseVersion>();

    public virtual SurveyQuestion SurveyQuestion { get; set; } = null!;

    public virtual SurveyTakenResult SurveyTakenResult { get; set; } = null!;

    public virtual ICollection<DataPurchase> DataPurchases { get; set; } = new List<DataPurchase>();
}
