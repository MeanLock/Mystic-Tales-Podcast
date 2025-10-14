using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingStatusTracking
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public int BookingStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual BookingStatus BookingStatus { get; set; } = null!;
}
