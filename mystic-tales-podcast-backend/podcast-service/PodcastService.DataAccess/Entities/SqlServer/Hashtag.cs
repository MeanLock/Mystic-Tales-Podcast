using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class Hashtag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastChannelHashtag> PodcastChannelHashtags { get; set; } = new List<PodcastChannelHashtag>();

    public virtual ICollection<PodcastEpisodeHashtag> PodcastEpisodeHashtags { get; set; } = new List<PodcastEpisodeHashtag>();

    public virtual ICollection<PodcastShowHashtag> PodcastShowHashtags { get; set; } = new List<PodcastShowHashtag>();
}
