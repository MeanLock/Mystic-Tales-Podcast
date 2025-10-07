using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.sqlserver;

public partial class BookingOptionalManualCancelReason
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
