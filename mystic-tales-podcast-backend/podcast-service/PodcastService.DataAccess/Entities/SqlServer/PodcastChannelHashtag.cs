using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastChannelHashtag
{
    public Guid PodcastChannelId { get; set; }

    public int HashtagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Hashtag Hashtag { get; set; } = null!;

    public virtual PodcastChannel PodcastChannel { get; set; } = null!;
}
