using System;
using System.Collections.Generic;

namespace Architecture_1.DataAccess.Entities;

public partial class JobType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
