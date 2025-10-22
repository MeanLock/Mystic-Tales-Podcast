using System.Collections.Generic;
using UserService.BusinessLogic.DTOs.FilterTag;
using UserService.BusinessLogic.DTOs.Survey.Details;

namespace UserService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
