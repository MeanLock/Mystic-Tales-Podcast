using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class FilterTag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string TagColor { get; set; } = null!;

    public int FilterTagTypeId { get; set; }

    public virtual FilterTagType FilterTagType { get; set; } = null!;

    public virtual ICollection<SurveyTagFilter> SurveyTagFilters { get; set; } = new List<SurveyTagFilter>();

    public virtual ICollection<SurveyTakenResultTagFilter> SurveyTakenResultTagFilters { get; set; } = new List<SurveyTakenResultTagFilter>();

    public virtual ICollection<TakerTagFilter> TakerTagFilters { get; set; } = new List<TakerTagFilter>();
}
