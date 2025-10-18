using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyDefaultBackgroundTheme
{
    public int Id { get; set; }

    public string ConfigJsonString { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
