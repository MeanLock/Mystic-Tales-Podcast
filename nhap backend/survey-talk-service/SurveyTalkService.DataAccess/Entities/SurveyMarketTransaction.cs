using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyMarketTransaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int DataPurchaseId { get; set; }

    public decimal Amount { get; set; }

    public decimal? Profit { get; set; }

    public int TransactionTypeId { get; set; }

    public int TransactionStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual DataPurchase DataPurchase { get; set; } = null!;

    public virtual TransactionStatus TransactionStatus { get; set; } = null!;

    public virtual TransactionType TransactionType { get; set; } = null!;
}
