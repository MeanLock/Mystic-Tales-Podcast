using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveySpecificTopic
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int SurveyTopicId { get; set; }

    public virtual SurveyTopic SurveyTopic { get; set; } = null!;

    public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();
}
