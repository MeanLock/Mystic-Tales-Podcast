using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingChatMessage
{
    public Guid Id { get; set; }

    public string? Text { get; set; }

    public string? AudioFileKey { get; set; }

    public Guid ChatRoomId { get; set; }

    public int SenderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BookingChatRoom ChatRoom { get; set; } = null!;
}
