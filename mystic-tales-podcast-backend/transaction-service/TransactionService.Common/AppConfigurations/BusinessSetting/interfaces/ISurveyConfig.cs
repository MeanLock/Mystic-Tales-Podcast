using Newtonsoft.Json.Linq;

namespace TransactionService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface ISurveyConfig
    {
        float KpiExceedXpEarnRate { get; set; }
        int RecentTakenThreshold { get; set; }
        JObject DefaultSurveyConfigJson { get; set; }
    }
}
