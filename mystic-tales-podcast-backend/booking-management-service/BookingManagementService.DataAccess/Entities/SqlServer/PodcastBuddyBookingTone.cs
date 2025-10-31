using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class PodcastBuddyBookingTone
{
    public int PodcasterId { get; set; }

    public Guid PodcastBookingToneId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PodcastBookingTone PodcastBookingTone { get; set; } = null!;
}
