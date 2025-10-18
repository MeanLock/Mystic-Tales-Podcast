
using TransactionService.BusinessLogic.DTOs.FilterTag;

namespace TransactionService.BusinessLogic.DTOs.Survey.Details
{
    public class SummarizedAndEmbeddingVectorFilterTagDTO : FilterTagDTO
    {
        public string? Summary { get; set; }
        public float[]? EmbeddingVector { get; set; }
    }
}
