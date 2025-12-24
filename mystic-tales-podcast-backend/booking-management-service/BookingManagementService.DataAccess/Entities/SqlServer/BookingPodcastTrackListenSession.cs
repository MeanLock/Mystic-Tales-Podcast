using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingPodcastTrackListenSession
{
    public Guid Id { get; set; }

    public int AccountId { get; set; }

    public Guid BookingPodcastTrackId { get; set; }

    public int LastListenDurationSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BookingPodcastTrack BookingPodcastTrack { get; set; } = null!;
}
