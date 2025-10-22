using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class Report
{
    public int Id { get; set; }

    public int ReportTypeId { get; set; }

    public int AccountId { get; set; }

    public int FacilityMajorId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsResolved { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual FacilityMajor FacilityMajor { get; set; } = null!;

    public virtual ReportType ReportType { get; set; } = null!;
}
