using System;
using System.Collections.Generic;

namespace SubscriptionService.DataAccess.Entities.sqlserver;

public partial class PodcastSubscription
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid? PodcastChannelId { get; set; }

    public Guid? PodcastShowId { get; set; }

    public bool IsActive { get; set; }

    public int CurrentVersion { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<PodcastSubscriptionBenefitMapping> PodcastSubscriptionBenefitMappings { get; set; } = new List<PodcastSubscriptionBenefitMapping>();

    public virtual ICollection<PodcastSubscriptionCycleTypePrice> PodcastSubscriptionCycleTypePrices { get; set; } = new List<PodcastSubscriptionCycleTypePrice>();

    public virtual ICollection<PodcastSubscriptionRegistration> PodcastSubscriptionRegistrations { get; set; } = new List<PodcastSubscriptionRegistration>();
}
