using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingStatus
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<BookingStatusTracking> BookingStatusTrackings { get; set; } = new List<BookingStatusTracking>();
}
