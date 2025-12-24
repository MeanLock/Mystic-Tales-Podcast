using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.SqlServer;

public partial class MemberSubscription
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsSubscribable { get; set; }

    public int CurrentVersion { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<MemberSubscriptionBenefitMapping> MemberSubscriptionBenefitMappings { get; set; } = new List<MemberSubscriptionBenefitMapping>();

    public virtual ICollection<MemberSubscriptionCycleTypePrice> MemberSubscriptionCycleTypePrices { get; set; } = new List<MemberSubscriptionCycleTypePrice>();

    public virtual ICollection<MemberSubscriptionRegistration> MemberSubscriptionRegistrations { get; set; } = new List<MemberSubscriptionRegistration>();
}
