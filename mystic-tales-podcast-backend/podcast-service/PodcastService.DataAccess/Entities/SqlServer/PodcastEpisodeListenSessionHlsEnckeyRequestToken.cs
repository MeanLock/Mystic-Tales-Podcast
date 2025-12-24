using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeListenSessionHlsEnckeyRequestToken
{
    public Guid PodcastEpisodeListenSessionId { get; set; }

    public string Token { get; set; } = null!;

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisodeListenSession PodcastEpisodeListenSession { get; set; } = null!;
}
