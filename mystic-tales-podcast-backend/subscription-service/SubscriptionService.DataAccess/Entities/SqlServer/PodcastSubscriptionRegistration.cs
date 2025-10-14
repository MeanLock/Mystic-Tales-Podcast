using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.SqlServer;

public partial class PodcastSubscriptionRegistration
{
    public int AccountId { get; set; }

    public int PodcastSubscriptionId { get; set; }

    public int SubscriptionCycleTypeId { get; set; }

    public int CurrentVersion { get; set; }

    public bool? IsAcceptNewestVersionSwitch { get; set; }

    public DateTime LastPaidAt { get; set; }

    public bool IsIncomeTaken { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PodcastSubscription PodcastSubscription { get; set; } = null!;

    public virtual SubscriptionCycleType SubscriptionCycleType { get; set; } = null!;

    public virtual ICollection<PodcastSubscriptionBenefit> PodcastSubscriptionBenefits { get; set; } = new List<PodcastSubscriptionBenefit>();
}
