
using SystemConfigurationService.BusinessLogic.DTOs.FilterTag;

namespace SystemConfigurationService.BusinessLogic.DTOs.Survey.Details
{
    public class SummarizedAndEmbeddingVectorFilterTagDTO : FilterTagDTO
    {
        public string? Summary { get; set; }
        public float[]? EmbeddingVector { get; set; }
    }
}
