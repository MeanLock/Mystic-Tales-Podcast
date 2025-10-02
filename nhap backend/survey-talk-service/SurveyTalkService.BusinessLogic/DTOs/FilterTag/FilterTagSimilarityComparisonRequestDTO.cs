using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.FilterTag
{
    public class CandidateEmbeddingVectorFilterTagsDTO
    {
        public int CandidateId { get; set; }
        public List<EmbeddingVectorFilterTagDTO> EmbeddingVectorFilterTags { get; set; } = new();
        public double? CandidateTagFilterAccuracyRate { get; set; }

    }

    public class FilterTagSimilarityComparisonRequestDTO
    {
        public List<EmbeddingVectorFilterTagDTO> TargetEmbeddingVectorFilterTags { get; set; } = new();
        public List<CandidateEmbeddingVectorFilterTagsDTO> CandidateEmbeddingVectorFilterTags { get; set; } = new();
        public float MinScore { get; set; }
        public float MaxScore { get; set; }
        public double? TargetTagFilterAccuracyRate { get; set; }
    }
}