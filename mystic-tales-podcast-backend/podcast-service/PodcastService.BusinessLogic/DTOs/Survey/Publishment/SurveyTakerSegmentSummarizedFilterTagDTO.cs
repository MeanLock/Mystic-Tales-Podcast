using System.Collections.Generic;
using PodcastService.BusinessLogic.DTOs.FilterTag;
using PodcastService.BusinessLogic.DTOs.Survey.Details;

namespace PodcastService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
