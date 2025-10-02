using System.Collections.Generic;
using SubscriptionService.BusinessLogic.DTOs.FilterTag;
using SubscriptionService.BusinessLogic.DTOs.Survey.Details;

namespace SubscriptionService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
