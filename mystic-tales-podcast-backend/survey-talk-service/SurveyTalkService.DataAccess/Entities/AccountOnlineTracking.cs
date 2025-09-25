using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class AccountOnlineTracking
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public DateOnly OnlineDate { get; set; }

    public int SurveyTakenCount { get; set; }

    public virtual Account Account { get; set; } = null!;
}
