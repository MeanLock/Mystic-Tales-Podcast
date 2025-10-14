using System;
using System.Collections.Generic;

namespace PodcastService.DataAccess.Entities.SqlServer;

public partial class PodcastCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastChannel> PodcastChannels { get; set; } = new List<PodcastChannel>();

    public virtual ICollection<PodcastShow> PodcastShows { get; set; } = new List<PodcastShow>();

    public virtual ICollection<PodcastSubCategory> PodcastSubCategories { get; set; } = new List<PodcastSubCategory>();
}
