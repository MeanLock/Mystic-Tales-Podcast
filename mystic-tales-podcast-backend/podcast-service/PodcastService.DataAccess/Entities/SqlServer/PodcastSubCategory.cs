using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastSubCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int PodcastCategoryId { get; set; }

    public virtual PodcastCategory PodcastCategory { get; set; } = null!;

    public virtual ICollection<PodcastChannel> PodcastChannels { get; set; } = new List<PodcastChannel>();

    public virtual ICollection<PodcastEpisodeListenSession> PodcastEpisodeListenSessions { get; set; } = new List<PodcastEpisodeListenSession>();

    public virtual ICollection<PodcastShow> PodcastShows { get; set; } = new List<PodcastShow>();
}
