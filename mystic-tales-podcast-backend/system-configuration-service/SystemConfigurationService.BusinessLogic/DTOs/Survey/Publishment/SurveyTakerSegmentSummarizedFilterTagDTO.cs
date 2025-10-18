using System.Collections.Generic;
using SystemConfigurationService.BusinessLogic.DTOs.FilterTag;
using SystemConfigurationService.BusinessLogic.DTOs.Survey.Details;

namespace SystemConfigurationService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
