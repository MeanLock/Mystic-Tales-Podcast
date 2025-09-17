using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class ReportType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
