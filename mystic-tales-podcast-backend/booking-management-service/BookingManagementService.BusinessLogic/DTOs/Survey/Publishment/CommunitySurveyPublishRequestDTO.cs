using System;
using System.Collections.Generic;
using BookingManagementService.BusinessLogic.DTOs.FilterTag;
using BookingManagementService.BusinessLogic.DTOs.Survey.Details;

namespace BookingManagementService.BusinessLogic.DTOs.Survey.Publishment
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
