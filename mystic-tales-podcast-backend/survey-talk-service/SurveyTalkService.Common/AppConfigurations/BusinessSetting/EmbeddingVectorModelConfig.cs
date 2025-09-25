using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SurveyTalkService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace SurveyTalkService.Common.AppConfigurations.BusinessSetting
{
    public class EmbeddingVectorModelConfigModel
    {
        public float MinScore { get; set; }
        public float MaxScore { get; set; }
        
    }
    public class EmbeddingVectorModelConfig : IEmbeddingVectorModelConfig
    {
        public float MinScore { get; set; }
        public float MaxScore { get; set; }


        public EmbeddingVectorModelConfig(IConfiguration configuration)
        {
            var embeddingVectorModelConfig = configuration.GetSection("BusinessSettings:EmbeddingVectorModel").Get<EmbeddingVectorModelConfigModel>();
            MinScore = embeddingVectorModelConfig.MinScore;
            MaxScore = embeddingVectorModelConfig.MaxScore;
        }
    }
}
