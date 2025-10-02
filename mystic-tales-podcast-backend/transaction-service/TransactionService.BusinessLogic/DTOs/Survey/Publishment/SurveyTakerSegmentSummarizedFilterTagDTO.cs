using System.Collections.Generic;
using TransactionService.BusinessLogic.DTOs.FilterTag;
using TransactionService.BusinessLogic.DTOs.Survey.Details;

namespace TransactionService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
