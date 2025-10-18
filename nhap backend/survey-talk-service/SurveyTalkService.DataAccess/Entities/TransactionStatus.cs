using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class TransactionStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AccountBalanceTransaction> AccountBalanceTransactions { get; set; } = new List<AccountBalanceTransaction>();

    public virtual ICollection<SurveyCommunityTransaction> SurveyCommunityTransactions { get; set; } = new List<SurveyCommunityTransaction>();

    public virtual ICollection<SurveyMarketTransaction> SurveyMarketTransactions { get; set; } = new List<SurveyMarketTransaction>();
}
