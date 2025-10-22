using Newtonsoft.Json.Linq;

namespace ModerationService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface ISurveyConfig
    {
        float KpiExceedXpEarnRate { get; set; }
        int RecentTakenThreshold { get; set; }
        JObject DefaultSurveyConfigJson { get; set; }
    }
}
