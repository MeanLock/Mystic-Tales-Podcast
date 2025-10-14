using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeStatusTracking
{
    public Guid Id { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public int PodcastEpisodeStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisode PodcastEpisode { get; set; } = null!;

    public virtual PodcastEpisodeStatus PodcastEpisodeStatus { get; set; } = null!;
}
