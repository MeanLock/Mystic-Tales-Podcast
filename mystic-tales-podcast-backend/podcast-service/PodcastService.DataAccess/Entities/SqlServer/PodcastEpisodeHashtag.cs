using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeHashtag
{
    public Guid PodcastEpisodeId { get; set; }

    public int HashtagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Hashtag Hashtag { get; set; } = null!;

    public virtual PodcastEpisode PodcastEpisode { get; set; } = null!;
}
