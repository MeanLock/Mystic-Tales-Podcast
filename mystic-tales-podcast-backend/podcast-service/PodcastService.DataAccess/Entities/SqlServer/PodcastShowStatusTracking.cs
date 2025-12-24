using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastShowStatusTracking
{
    public Guid Id { get; set; }

    public Guid PodcastShowId { get; set; }

    public int PodcastShowStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastShow PodcastShow { get; set; } = null!;

    public virtual PodcastShowStatus PodcastShowStatus { get; set; } = null!;
}
