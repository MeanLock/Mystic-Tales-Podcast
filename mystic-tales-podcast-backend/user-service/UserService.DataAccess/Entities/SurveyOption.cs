using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities;

public partial class SurveyOption
{
    public string Content { get; set; } = null!;

    public byte Order { get; set; }

    public Guid Id { get; set; }

    public Guid SurveyQuestionId { get; set; }

    public virtual SurveyQuestion SurveyQuestion { get; set; } = null!;
}
