using System.Collections.Generic;
using ModerationService.BusinessLogic.DTOs.FilterTag;
using ModerationService.BusinessLogic.DTOs.Survey.Details;

namespace ModerationService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
