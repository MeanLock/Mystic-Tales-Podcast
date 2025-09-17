using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class FacilityMajorType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<FacilityMajor> FacilityMajors { get; set; } = new List<FacilityMajor>();
}
