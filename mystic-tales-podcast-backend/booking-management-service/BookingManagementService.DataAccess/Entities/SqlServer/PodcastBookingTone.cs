using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class PodcastBookingTone
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int PodcastBookingToneCategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<BookingRequirement> BookingRequirements { get; set; } = new List<BookingRequirement>();

    public virtual PodcastBookingToneCategory PodcastBookingToneCategory { get; set; }

    public virtual ICollection<PodcastBuddyBookingTone> PodcastBuddyBookingTones { get; set; } = new List<PodcastBuddyBookingTone>();
}
