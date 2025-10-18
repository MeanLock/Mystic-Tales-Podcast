using Newtonsoft.Json.Linq;

namespace TransactionService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IEmbeddingVectorModelConfig
    {
        float MinScore { get; set; }
        float MaxScore { get; set; }
    }
}
