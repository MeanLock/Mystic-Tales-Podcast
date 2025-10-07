using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.sqlserver;

public partial class SubscriptionCycleType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<MemberSubscriptionCycleTypePrice> MemberSubscriptionCycleTypePrices { get; set; } = new List<MemberSubscriptionCycleTypePrice>();

    public virtual ICollection<PodcastSubscriptionCycleTypePrice> PodcastSubscriptionCycleTypePrices { get; set; } = new List<PodcastSubscriptionCycleTypePrice>();

    public virtual ICollection<PodcastSubscriptionRegistration> PodcastSubscriptionRegistrations { get; set; } = new List<PodcastSubscriptionRegistration>();
}
