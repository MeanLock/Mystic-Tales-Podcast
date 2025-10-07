using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.sqlserver;

public partial class MemberSubscriptionBenefitMapping
{
    public int MemberSubscriptionId { get; set; }

    public int MemberSubscriptionBenefitId { get; set; }

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual MemberSubscription MemberSubscription { get; set; } = null!;

    public virtual MemberSubscriptionBenefit MemberSubscriptionBenefit { get; set; } = null!;
}
