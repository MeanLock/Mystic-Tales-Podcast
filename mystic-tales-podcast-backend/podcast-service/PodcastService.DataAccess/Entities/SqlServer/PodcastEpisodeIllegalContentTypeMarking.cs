using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastEpisodeIllegalContentTypeMarking
{
    public Guid PodcastEpisodeId { get; set; }

    public int PodcastIllegalContentTypeId { get; set; }

    public int? MarkerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastEpisode PodcastEpisode { get; set; } = null!;

    public virtual PodcastIllegalContentType PodcastIllegalContentType { get; set; } = null!;
}
