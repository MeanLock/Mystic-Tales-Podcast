
using SubscriptionService.BusinessLogic.DTOs.FilterTag;

namespace SubscriptionService.BusinessLogic.DTOs.Survey.Details
{
    public class SummarizedAndEmbeddingVectorFilterTagDTO : FilterTagDTO
    {
        public string? Summary { get; set; }
        public float[]? EmbeddingVector { get; set; }
    }
}
