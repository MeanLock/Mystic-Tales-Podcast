using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities.SqlServer;

public partial class PodcastRestrictedTerm
{
    public int Id { get; set; }

    public string Term { get; set; } = null!;
}
