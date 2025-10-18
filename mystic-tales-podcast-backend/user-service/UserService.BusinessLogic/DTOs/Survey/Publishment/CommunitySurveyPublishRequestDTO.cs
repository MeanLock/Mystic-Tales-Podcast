using System;
using System.Collections.Generic;
using UserService.BusinessLogic.DTOs.FilterTag;
using UserService.BusinessLogic.DTOs.Survey.Details;

namespace UserService.BusinessLogic.DTOs.Survey.Publishment
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
