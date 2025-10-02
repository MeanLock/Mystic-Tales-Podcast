using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class FilterTagType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<FilterTag> FilterTags { get; set; } = new List<FilterTag>();
}
