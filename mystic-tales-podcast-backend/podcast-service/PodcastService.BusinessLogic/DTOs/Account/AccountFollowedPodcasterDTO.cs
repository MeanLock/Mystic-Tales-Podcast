using System;
using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.Account;

public partial class AccountFollowedPodcasterDTO
{
    public int AccountId { get; set; }

    public int PodcasterId { get; set; }

    public DateTime CreatedAt { get; set; }

    public AccountDTO Account { get; set; } = null!;

    public AccountDTO Podcaster { get; set; } = null!;
}
