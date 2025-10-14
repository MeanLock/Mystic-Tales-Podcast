using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class Hashtag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastChannel> PodcastChannels { get; set; } = new List<PodcastChannel>();

    public virtual ICollection<PodcastEpisode> PodcastEpisodes { get; set; } = new List<PodcastEpisode>();

    public virtual ICollection<PodcastShow> PodcastShows { get; set; } = new List<PodcastShow>();
}
