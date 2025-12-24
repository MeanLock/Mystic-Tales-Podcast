using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastShowHashtag
{
    public Guid PodcastShowId { get; set; }

    public int HashtagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Hashtag Hashtag { get; set; } = null!;

    public virtual PodcastShow PodcastShow { get; set; } = null!;
}
