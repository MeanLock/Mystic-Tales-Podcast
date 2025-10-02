using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class FacilityItemAssignment
{
    public int FacilityItemId { get; set; }

    public int FacilityMajorId { get; set; }

    public int ItemCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual FacilityItem FacilityItem { get; set; } = null!;

    public virtual FacilityMajor FacilityMajor { get; set; } = null!;
}
