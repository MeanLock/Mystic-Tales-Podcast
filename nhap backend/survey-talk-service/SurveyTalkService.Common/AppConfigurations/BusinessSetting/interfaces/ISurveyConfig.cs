using Newtonsoft.Json.Linq;

namespace SurveyTalkService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface ISurveyConfig
    {
        float KpiExceedXpEarnRate { get; set; }
        int RecentTakenThreshold { get; set; }
        JObject DefaultSurveyConfigJson { get; set; }
    }
}
