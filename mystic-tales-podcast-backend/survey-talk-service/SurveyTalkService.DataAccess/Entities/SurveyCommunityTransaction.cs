using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyCommunityTransaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int SurveyId { get; set; }

    public decimal Amount { get; set; }

    public decimal? Profit { get; set; }

    public int TransactionTypeId { get; set; }

    public int TransactionStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Survey Survey { get; set; } = null!;

    public virtual TransactionStatus TransactionStatus { get; set; } = null!;

    public virtual TransactionType TransactionType { get; set; } = null!;
}
