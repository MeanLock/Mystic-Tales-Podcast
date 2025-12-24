using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities.SqlServer;

public partial class AccountFavoritedPodcastChannel
{
    public int AccountId { get; set; }

    public Guid PodcastChannelId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
