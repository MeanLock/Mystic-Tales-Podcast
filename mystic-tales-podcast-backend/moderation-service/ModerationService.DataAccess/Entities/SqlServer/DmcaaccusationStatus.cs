using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class DmcaaccusationStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<DmcaaccusationStatusTracking> DmcaaccusationStatusTrackings { get; set; } = new List<DmcaaccusationStatusTracking>();
}
