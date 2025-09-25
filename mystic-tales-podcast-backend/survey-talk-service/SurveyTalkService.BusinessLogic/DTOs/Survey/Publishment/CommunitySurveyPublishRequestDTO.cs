using System;
using System.Collections.Generic;
using SurveyTalkService.BusinessLogic.DTOs.FilterTag;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Details;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.Publishment
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
