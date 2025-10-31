using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities.SqlServer;

public partial class DmcaaccusationConclusionReportType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<DmcaaccusationConclusionReport> DmcaaccusationConclusionReports { get; set; } = new List<DmcaaccusationConclusionReport>();
}
