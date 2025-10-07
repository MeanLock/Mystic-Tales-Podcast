
using UserService.BusinessLogic.DTOs.FilterTag;

namespace UserService.BusinessLogic.DTOs.Survey.Details
{
    public class SummarizedAndEmbeddingVectorFilterTagDTO : FilterTagDTO
    {
        public string? Summary { get; set; }
        public float[]? EmbeddingVector { get; set; }
    }
}
