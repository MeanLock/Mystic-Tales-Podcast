using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class PasswordResetToken
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
