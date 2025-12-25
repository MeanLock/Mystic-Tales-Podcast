using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingStatusTrackingDTO
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public int BookingStatusId { get; set; }

    public DateTime CreatedAt { get; set; }
}
