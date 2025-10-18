using System;
using System.Collections.Generic;
using PodcastService.BusinessLogic.DTOs.FilterTag;
using PodcastService.BusinessLogic.DTOs.Survey.Details;

namespace PodcastService.BusinessLogic.DTOs.Survey.Publishment
{
    public class CommunitySurveyPublishRequestDTO
    {
        public int Kpi { get; set; }
        public DateTime EndDate { get; set; }
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; } = new();
        public SurveyTakerSegmentDTO SurveyTakerSegment { get; set; } = new();
        public decimal ExtraPrice { get; set; } = 0;
        public decimal TheoryPrice { get; set; }
    }
}
