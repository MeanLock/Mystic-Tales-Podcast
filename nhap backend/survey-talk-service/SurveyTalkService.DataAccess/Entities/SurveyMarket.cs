using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SurveyMarket
{
    public int SurveyId { get; set; }

    public byte Version { get; set; }

    public string? Description { get; set; }

    public decimal? PricePerResponse { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime? PublishAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Survey Survey { get; set; } = null!;
}
