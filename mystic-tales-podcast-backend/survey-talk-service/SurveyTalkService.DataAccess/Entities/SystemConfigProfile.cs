using System;
using System.Collections.Generic;

namespace SurveyTalkService.DataAccess.Entities;

public partial class SystemConfigProfile
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AccountGeneralConfig? AccountGeneralConfig { get; set; }

    public virtual ICollection<AccountLevelSettingConfig> AccountLevelSettingConfigs { get; set; } = new List<AccountLevelSettingConfig>();

    public virtual SurveyGeneralConfig? SurveyGeneralConfig { get; set; }

    public virtual SurveyMarketConfig? SurveyMarketConfig { get; set; }

    public virtual ICollection<SurveySecurityModeConfig> SurveySecurityModeConfigs { get; set; } = new List<SurveySecurityModeConfig>();

    public virtual ICollection<SurveyTimeRateConfig> SurveyTimeRateConfigs { get; set; } = new List<SurveyTimeRateConfig>();
}
