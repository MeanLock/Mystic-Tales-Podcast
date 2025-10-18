using System;
using System.Collections.Generic;

namespace SystemConfigurationService.DataAccess.Entities;

public partial class SurveyDefaultBackgroundTheme
{
    public int Id { get; set; }

    public string ConfigJsonString { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
