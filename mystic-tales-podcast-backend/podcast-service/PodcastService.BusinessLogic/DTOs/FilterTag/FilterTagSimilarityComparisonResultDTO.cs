using System.Collections.Generic;

namespace PodcastService.BusinessLogic.DTOs.FilterTag
{
    public class FilterTagSimilarityComparisonResultDTO
    {
        public int CandidateId { get; set; }
        public float SimilarityScore { get; set; }

    }
}