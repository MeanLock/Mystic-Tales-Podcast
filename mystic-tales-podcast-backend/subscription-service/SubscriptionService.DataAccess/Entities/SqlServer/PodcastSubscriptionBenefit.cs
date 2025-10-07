using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.sqlserver;

public partial class PodcastSubscriptionBenefit
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastSubscriptionBenefitMapping> PodcastSubscriptionBenefitMappings { get; set; } = new List<PodcastSubscriptionBenefitMapping>();

    public virtual ICollection<PodcastSubscriptionRegistration> PodcastSubscriptionRegistrations { get; set; } = new List<PodcastSubscriptionRegistration>();
}
