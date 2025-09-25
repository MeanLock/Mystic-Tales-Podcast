using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class Account
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public string? FullName { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public decimal Balance { get; set; }

    public bool IsVerified { get; set; }

    public int Xp { get; set; }

    public int Level { get; set; }

    public int ProgressionSurveyCount { get; set; }

    public bool IsFilterSurveyRequired { get; set; }

    public DateTime? LastFilterSurveyTakenAt { get; set; }

    public DateTime? DeactivatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? GoogleId { get; set; }

    public string? VerifyCode { get; set; }

    public virtual ICollection<AccountBalanceTransaction> AccountBalanceTransactions { get; set; } = new List<AccountBalanceTransaction>();

    public virtual AccountNationalVerification? AccountNationalVerification { get; set; }

    public virtual ICollection<AccountOnlineTracking> AccountOnlineTrackings { get; set; } = new List<AccountOnlineTracking>();

    public virtual AccountProfile? AccountProfile { get; set; }

    public virtual ICollection<DataPurchase> DataPurchases { get; set; } = new List<DataPurchase>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual PlatformFeedback? PlatformFeedback { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SurveyCommunityTransaction> SurveyCommunityTransactions { get; set; } = new List<SurveyCommunityTransaction>();

    public virtual ICollection<SurveyFeedback> SurveyFeedbacks { get; set; } = new List<SurveyFeedback>();

    public virtual ICollection<SurveyMarketTransaction> SurveyMarketTransactions { get; set; } = new List<SurveyMarketTransaction>();

    public virtual ICollection<SurveyTakenResult> SurveyTakenResults { get; set; } = new List<SurveyTakenResult>();

    public virtual ICollection<SurveyTopicFavorite> SurveyTopicFavorites { get; set; } = new List<SurveyTopicFavorite>();

    public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();

    public virtual ICollection<TakerTagFilter> TakerTagFilters { get; set; } = new List<TakerTagFilter>();
}
