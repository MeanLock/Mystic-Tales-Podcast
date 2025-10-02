using System.Collections.Generic;

namespace SubscriptionService.BusinessLogic.DTOs.FilterTag
{
    public class FilterTagSimilarityComparisonResultDTO
    {
        public int CandidateId { get; set; }
        public float SimilarityScore { get; set; }

    }
}