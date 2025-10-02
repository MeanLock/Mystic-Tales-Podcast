using System;
using System.Collections.Generic;

namespace ModerationService.DataAccess.Entities;

public partial class SurveyFieldInputType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
