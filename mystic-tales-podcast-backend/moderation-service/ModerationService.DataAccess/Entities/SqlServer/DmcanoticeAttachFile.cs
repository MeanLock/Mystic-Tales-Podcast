using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class DmcanoticeAttachFile
{
    public Guid Id { get; set; }

    public Guid DmcaNoticeId { get; set; }

    public string AttachFileKey { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Dmcanotice DmcaNotice { get; set; } = null!;
}
