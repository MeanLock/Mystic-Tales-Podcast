using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.sqlserver;

public partial class MemberSubscriptionBenefit
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<MemberSubscriptionBenefitMapping> MemberSubscriptionBenefitMappings { get; set; } = new List<MemberSubscriptionBenefitMapping>();
}
