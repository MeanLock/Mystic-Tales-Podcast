using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.sqlserver;

public partial class BookingRequirementAttachFile
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public string AttachFileKey { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual Booking Booking { get; set; } = null!;
}
