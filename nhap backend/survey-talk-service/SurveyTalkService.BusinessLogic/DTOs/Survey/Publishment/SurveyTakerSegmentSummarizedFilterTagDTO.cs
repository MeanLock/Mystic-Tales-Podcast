using System.Collections.Generic;
using SurveyTalkService.BusinessLogic.DTOs.FilterTag;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Details;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
