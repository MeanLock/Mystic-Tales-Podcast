using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.sqlserver;

public partial class BookingChatMember
{
    public Guid ChatRoomId { get; set; }

    public int AccountId { get; set; }

    public virtual BookingChatRoom ChatRoom { get; set; } = null!;
}
