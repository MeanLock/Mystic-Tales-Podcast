using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingChatRoom
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<BookingChatMember> BookingChatMembers { get; set; } = new List<BookingChatMember>();

    public virtual ICollection<BookingChatMessage> BookingChatMessages { get; set; } = new List<BookingChatMessage>();
}
