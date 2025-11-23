using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class PodcastBookingToneCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PodcastBookingTone> PodcastBookingTones { get; set; } = new List<PodcastBookingTone>();
}
