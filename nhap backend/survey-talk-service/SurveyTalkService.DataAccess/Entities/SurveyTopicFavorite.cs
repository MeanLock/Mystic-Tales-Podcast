using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyTopicFavorite
{
    public int AccountId { get; set; }

    public int SurveyTopicId { get; set; }

    public byte FavoriteScore { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual SurveyTopic SurveyTopic { get; set; } = null!;
}
