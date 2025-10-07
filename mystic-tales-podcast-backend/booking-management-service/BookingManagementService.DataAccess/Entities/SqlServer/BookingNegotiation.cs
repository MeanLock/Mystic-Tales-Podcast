using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.sqlserver;

public partial class BookingNegotiation
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public string Note { get; set; } = null!;

    public DateOnly? Deadline { get; set; }

    public decimal? Price { get; set; }

    public bool DemoAudioRequired { get; set; }

    public string? DemoAudioFileKey { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsFromCustomer { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
