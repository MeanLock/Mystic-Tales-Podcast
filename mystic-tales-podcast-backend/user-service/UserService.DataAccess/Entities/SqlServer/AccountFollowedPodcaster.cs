using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities.SqlServer;

public partial class AccountFollowedPodcaster
{
    public int AccountId { get; set; }

    public int PodcasterId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Account Podcaster { get; set; } = null!;
}
