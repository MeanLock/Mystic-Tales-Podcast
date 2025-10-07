using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.sqlserver;

public partial class PodcastIllegalContentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastEpisodeIllegalContentTypeMarking> PodcastEpisodeIllegalContentTypeMarkings { get; set; } = new List<PodcastEpisodeIllegalContentTypeMarking>();
}
