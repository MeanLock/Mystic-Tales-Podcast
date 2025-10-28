using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities.SqlServer;

public partial class NotificationType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AccountNotification> AccountNotifications { get; set; } = new List<AccountNotification>();
}
