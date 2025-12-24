using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastChannelStatusTracking
{
    public Guid Id { get; set; }

    public Guid PodcastChannelId { get; set; }

    public int PodcastChannelStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastChannel PodcastChannel { get; set; } = null!;

    public virtual PodcastChannelStatus PodcastChannelStatus { get; set; } = null!;
}
