using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class AssigneeFacilityMajorAssignment
{
    public int AccountId { get; set; }

    public int FacilityMajorId { get; set; }

    public bool IsHead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? WorkDescription { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual FacilityMajor FacilityMajor { get; set; } = null!;
}
