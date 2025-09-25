using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class DataPurchaseDetail
{
    public int DataPurchaseId { get; set; }

    public int SurveyResponseId { get; set; }

    public decimal PurchasedPrice { get; set; }

    public virtual DataPurchase DataPurchase { get; set; } = null!;

    public virtual SurveyResponse SurveyResponse { get; set; } = null!;
}
