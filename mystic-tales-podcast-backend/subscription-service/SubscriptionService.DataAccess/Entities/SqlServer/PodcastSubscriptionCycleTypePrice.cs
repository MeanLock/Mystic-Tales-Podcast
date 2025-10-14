using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.SqlServer;

public partial class PodcastSubscriptionCycleTypePrice
{
    public int PodcastSubscriptionId { get; set; }

    public int SubscriptionCycleTypeId { get; set; }

    public int Version { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PodcastSubscription PodcastSubscription { get; set; } = null!;

    public virtual SubscriptionCycleType SubscriptionCycleType { get; set; } = null!;
}
