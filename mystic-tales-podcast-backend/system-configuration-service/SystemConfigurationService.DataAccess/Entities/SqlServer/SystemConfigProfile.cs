using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities.sqlserver;

public partial class SystemConfigProfile
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AccountConfig? AccountConfig { get; set; }

    public virtual ICollection<AccountViolationLevelConfig> AccountViolationLevelConfigs { get; set; } = new List<AccountViolationLevelConfig>();

    public virtual BookingConfig? BookingConfig { get; set; }

    public virtual ICollection<PodcastSubscriptionConfig> PodcastSubscriptionConfigs { get; set; } = new List<PodcastSubscriptionConfig>();

    public virtual PodcastSuggestionConfig? PodcastSuggestionConfig { get; set; }

    public virtual ReviewSessionConfig? ReviewSessionConfig { get; set; }
}
