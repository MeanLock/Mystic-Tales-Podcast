using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.sqlserver;

public partial class DmcaaccusationStatusTracking
{
    public Guid Id { get; set; }

    public int DmcaAccusationId { get; set; }

    public int DmcaAccusationStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Dmcaaccusation DmcaAccusation { get; set; } = null!;

    public virtual DmcaaccusationStatus DmcaAccusationStatus { get; set; } = null!;
}
