using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities.SqlServer;

public partial class AccountNotification
{
    public Guid Id { get; set; }

    public int AccountId { get; set; }

    public string Content { get; set; } = null!;

    public int NotificationTypeId { get; set; }

    public bool IsSeen { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
