using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeListenSession
{
    public Guid Id { get; set; }

    public int AccountId { get; set; }

    public Guid PodcastEpisodeId { get; set; }

    public int LastListenDurationSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisode PodcastEpisode { get; set; } = null!;
}
