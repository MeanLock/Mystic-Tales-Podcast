using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.SqlServer;

public partial class MemberSubscriptionRegistration
{
    public int AccountId { get; set; }

    public int MemberSubscriptionId { get; set; }

    public int CurrentVersion { get; set; }

    public bool? IsAcceptNewestVersionSwitch { get; set; }

    public DateTime LastPaidAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual MemberSubscription MemberSubscription { get; set; } = null!;
}
