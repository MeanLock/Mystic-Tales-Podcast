using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class AccountVerification
{
    public int AccountId { get; set; }

    public int NationalCardNumber { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public DateTime VerifiedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
