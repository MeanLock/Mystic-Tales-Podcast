using System.Collections.Generic;
using BookingManagementService.BusinessLogic.DTOs.FilterTag;
using BookingManagementService.BusinessLogic.DTOs.Survey.Details;

namespace BookingManagementService.BusinessLogic.DTOs.Survey.Publishment
{
    public class SurveyTakerSegmentSummarizedFilterTagDTO
    {
        public List<SummarizedAndEmbeddingVectorFilterTagDTO> FilterTags { get; set; }
        public int? MaxKpi { get; set; }
        public float? R { get; set; }
    }


}
