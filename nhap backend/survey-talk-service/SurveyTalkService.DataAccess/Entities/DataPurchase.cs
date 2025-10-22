using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class DataPurchase
{
    public int Id { get; set; }

    public int BuyerId { get; set; }

    public int MarketSurveyId { get; set; }

    public byte Version { get; set; }

    public DateTime PurchasedAt { get; set; }

    public virtual Account Buyer { get; set; } = null!;

    public virtual Survey MarketSurvey { get; set; } = null!;

    public virtual ICollection<SurveyMarketTransaction> SurveyMarketTransactions { get; set; } = new List<SurveyMarketTransaction>();

    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
}
