using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.SqlServer;

public partial class PodcastSubscriptionBenefitMapping
{
    public int PodcastSubscriptionId { get; set; }

    public int PodcastSubscriptionBenefitId { get; set; }

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PodcastSubscription PodcastSubscription { get; set; } = null!;

    public virtual PodcastSubscriptionBenefit PodcastSubscriptionBenefit { get; set; } = null!;
}
