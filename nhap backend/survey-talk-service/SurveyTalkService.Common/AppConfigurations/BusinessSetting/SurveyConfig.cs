using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SurveyTalkService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace SurveyTalkService.Common.AppConfigurations.BusinessSetting
{
    public class SurveyConfigModel
    {
        public float KpiExceedXpEarnRate { get; set; }
        public int RecentTakenThreshold { get; set; }
        public JObject DefaultSurveyConfigJson { get; set; }
    }
    public class SurveyConfig : ISurveyConfig
    {
        public float KpiExceedXpEarnRate { get; set; }
        public int RecentTakenThreshold { get; set; }
        public JObject DefaultSurveyConfigJson { get; set; }

        public SurveyConfig(IConfiguration configuration)
        {
            // configuration.GetSection("FilePaths").Get<FilePathConfigModel>();
            // // Load KpiExceedXpEarnRate from configuration
            // var kpiExceedXpEarnRate = configuration.GetSection("BusinessSettings:Survey:KpiExceedXpEarnRate");

            // var DefaultSurveyConfigJson = configuration.GetSection("BusinessSettings:Survey:DefaultSurveyConfigJson");
            // if (DefaultSurveyConfigJson.Exists())
            // {
            //     var dict = DefaultSurveyConfigJson.GetChildren().ToDictionary(x => x.Key, x => x.Value);
            //     var jsonString = JsonConvert.SerializeObject(dict);
            //     this.DefaultSurveyConfigJson = JObject.Parse(jsonString);
            // }
            // else
            // {
            //     this.DefaultSurveyConfigJson = new JObject();
            // }


            var surveySection = configuration.GetSection("BusinessSettings:Survey");
            KpiExceedXpEarnRate = surveySection.GetValue<float>("KpiExceedXpEarnRate");
            RecentTakenThreshold = surveySection.GetValue<int>("RecentTakenThreshold");
            var defaultSurveyConfigSection = surveySection.GetSection("DefaultSurveyConfigJson");
            if (defaultSurveyConfigSection.Exists())
            {
                var dict = defaultSurveyConfigSection.GetChildren().ToDictionary(x => x.Key, x => x.Value != null ? JToken.FromObject(x.Value) : JValue.CreateNull());
                var jsonString = JsonConvert.SerializeObject(dict);
                this.DefaultSurveyConfigJson = JObject.Parse(jsonString);
            }
            else
            {
                this.DefaultSurveyConfigJson = new JObject();
            }

        }
    }
}
