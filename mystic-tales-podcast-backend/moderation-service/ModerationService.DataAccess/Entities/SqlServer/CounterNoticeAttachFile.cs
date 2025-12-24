using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class CounterNoticeAttachFile
{
    public Guid Id { get; set; }

    public Guid CounterNoticeId { get; set; }

    public string AttachFileKey { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual CounterNotice CounterNotice { get; set; } = null!;
}
