
using ModerationService.BusinessLogic.DTOs.FilterTag;

namespace ModerationService.BusinessLogic.DTOs.Survey.Details
{
    public class SummarizedAndEmbeddingVectorFilterTagDTO : FilterTagDTO
    {
        public string? Summary { get; set; }
        public float[]? EmbeddingVector { get; set; }
    }
}
