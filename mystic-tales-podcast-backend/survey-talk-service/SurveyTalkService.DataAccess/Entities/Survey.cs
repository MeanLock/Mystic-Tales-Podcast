using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class Survey
{
    public int Id { get; set; }

    public int RequesterId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int SurveyTypeId { get; set; }

    public int? SurveyTopicId { get; set; }

    public int? SurveySpecificTopicId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? Kpi { get; set; }

    public int? SecurityModeId { get; set; }

    public decimal? TheoryPrice { get; set; }

    public decimal? ExtraPrice { get; set; }

    public decimal? TakerBaseRewardPrice { get; set; }

    public decimal? ProfitPrice { get; set; }

    public decimal? AllocBaseAmount { get; set; }

    public decimal? AllocTimeAmount { get; set; }

    public decimal? AllocLevelAmount { get; set; }

    public int? MaxXp { get; set; }

    public bool IsAvailable { get; set; }

    public string ConfigJsonString { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DataPurchase> DataPurchases { get; set; } = new List<DataPurchase>();

    public virtual Account Requester { get; set; } = null!;

    public virtual SurveySecurityMode? SecurityMode { get; set; }

    public virtual ICollection<SurveyCommunityTransaction> SurveyCommunityTransactions { get; set; } = new List<SurveyCommunityTransaction>();

    public virtual ICollection<SurveyFeedback> SurveyFeedbacks { get; set; } = new List<SurveyFeedback>();

    public virtual ICollection<SurveyMarketVersionStatusTracking> SurveyMarketVersionStatusTrackings { get; set; } = new List<SurveyMarketVersionStatusTracking>();

    public virtual ICollection<SurveyMarket> SurveyMarkets { get; set; } = new List<SurveyMarket>();

    public virtual ICollection<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();

    public virtual ICollection<SurveyRewardTracking> SurveyRewardTrackings { get; set; } = new List<SurveyRewardTracking>();

    public virtual SurveySpecificTopic? SurveySpecificTopic { get; set; }

    public virtual ICollection<SurveyStatusTracking> SurveyStatusTrackings { get; set; } = new List<SurveyStatusTracking>();

    public virtual ICollection<SurveyTagFilter> SurveyTagFilters { get; set; } = new List<SurveyTagFilter>();

    public virtual ICollection<SurveyTakenResult> SurveyTakenResults { get; set; } = new List<SurveyTakenResult>();

    public virtual SurveyTakerSegment? SurveyTakerSegment { get; set; }

    public virtual SurveyTopic? SurveyTopic { get; set; }

    public virtual SurveyType SurveyType { get; set; } = null!;
}
