using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class ServiceType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
