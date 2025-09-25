using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyTopic
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<SurveySpecificTopic> SurveySpecificTopics { get; set; } = new List<SurveySpecificTopic>();

    public virtual ICollection<SurveyTopicFavorite> SurveyTopicFavorites { get; set; } = new List<SurveyTopicFavorite>();

    public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();
}
